﻿namespace Digital_Wallet.Controllers
{
    using Wallet.Common.Exceptions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using Wallet.Common.Helpers;
    using Wallet.Data.Helpers.Contracts;
    using Wallet.Data.Models;
    using Wallet.DTO.Request;
    using Wallet.Services.Contracts;

    namespace Digital_Wallet.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class UserController : ControllerBase
        {
            private readonly IUserService _userService;
            private readonly IAuthManager _authManager;
            private readonly IAccountService _accountService;
            private readonly ITwoFactorAuthService _twoFactorAuthService;

            public UserController(IUserService userService, IAuthManager authManager, IAccountService accountService, ITwoFactorAuthService twoFactorAuthService)
            {
                _userService = userService;
                _authManager = authManager;
                _accountService = accountService;
                _twoFactorAuthService = twoFactorAuthService;
            }

            [HttpPost("add-user")]
            public async Task<IActionResult> Register(RegisterModel model)
            {
                var result = await _userService.RegisterUserAsync(model);
                if (result.Succeeded)
                {
                    return Ok(Messages.Controller.RegistrationSuccess);
                }

                return BadRequest(result.Errors);
            }

            [HttpPost("verify-email")]
            public async Task<IActionResult> VerifyEmail(VerifyEmailModel model)
            {
                var isSuccess = await _userService.VerifyEmailAsync(model);
                if (isSuccess)
                {
                    return Ok(Messages.Controller.EmailConfirmationSuccess);
                }

                return BadRequest(Messages.Controller.InvalidOrExpiredConfirmationCode);
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginModel model)
            {
                var (user, requiresTwoFactor) = await _userService.LoginAsync(model);

                if (user == null)
                {
                    return Unauthorized(Messages.Controller.InvalidEmailOrPassword);
                }

                if (requiresTwoFactor)
                {
                    // 2FA is required, so return a response indicating this
                    return Ok(new { TwoFactorRequired = true, userId = user.Id });
                }

                // Generate the token if login is successful without 2FA
                var authToken = await _authManager.GenerateJwtToken(user);
                return Ok(new { token = authToken });
            }


            [HttpPost("verify-phone")]
            public async Task<IActionResult> VerifyPhone(string phoneNumber, string code)
            {
                var isSuccess = await _userService.VerifyPhoneAsync(phoneNumber, code);
                if (isSuccess)
                {
                    return Ok(Messages.Controller.PhoneNumberVerificationSuccess);
                }

                return BadRequest(Messages.Controller.InvalidVerificationCode);
            }

            [HttpPost("uploadProfilePicture")]
            [Authorize]
            public async Task<IActionResult> UploadProfilePicture(IFormFile file)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                try
                {
                    await _userService.UploadProfilePictureAsync(userId, file);
                    return Ok(new { message = Messages.Controller.ProfilePictureUpdatedSuccessfully });
                }
                catch (EntityNotFoundException ex)
                {
                    return NotFound(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
            }

            [HttpPost("update-profile")]
            [Authorize]
            public async Task<IActionResult> UpdateUserProfile(UpdateUserModel model)
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData);
                var result = await _userService.UpdateUserProfileAsync(model, userId);

                if (result.Succeeded)
                {
                    return Ok(Messages.Controller.UserProfileUpdatedSuccessfully);
                }

                return BadRequest(result.Errors);
            }

            [HttpGet("enable-2fa")]
            [Authorize]
            [Produces("image/png")]
            public async Task<IActionResult> Enable2FA()
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData);
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(Messages.UserNotFound);
                }

                var qrCodeImage = await _twoFactorAuthService.GenerateQrCodeImageAsync(user);
                return File(qrCodeImage, "image/png");
            }

            [HttpPost("verify-2fa")]
            [Authorize]
            public async Task<IActionResult> Verify2FA([FromBody] Verify2FAModel model)
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData);
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(Messages.UserNotFound);
                }

                var is2faTokenValid = await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, model.Code);

                if (!is2faTokenValid)
                {
                    return BadRequest(Messages.Controller.InvalidVerificationCode);
                }

                await _twoFactorAuthService.EnableTwoFactorAuthenticationAsync(user);

                return Ok(Messages.Controller.TwoFactorAuthenticationEnabled);
            }

            [HttpPost("login-2fa")]
            public async Task<IActionResult> Login2FA([FromBody] Login2FAModel model)
            {
                var user = await _userService.GetUserByIdAsync(model.UserId);
                if (user == null)
                {
                    return Unauthorized(Messages.UserNotFound);
                }

                var is2faTokenValid = await _userService.VerifyTwoFactorCodeAsync(user, model.Code);
                if (!is2faTokenValid)
                {
                    return Unauthorized(Messages.Controller.InvalidVerificationCode); //"Invalid 2FA code."
                }

                var authToken = await _authManager.GenerateJwtToken(user);
                return Ok(new { token = authToken });
            }

            [HttpPost("disable-2fa")]
            [Authorize]
            public async Task<IActionResult> Disable2FA()
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData);
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(Messages.UserNotFound);
                }

                await _twoFactorAuthService.DisableTwoFactorAuthenticationAsync(user);
                return Ok(Messages.Controller.TwoFactorAuthenticationDisabled);
            }
        }
    }
}
