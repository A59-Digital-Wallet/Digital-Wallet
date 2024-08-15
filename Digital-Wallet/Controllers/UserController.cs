using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Wallet.Common.Exceptions;


namespace Digital_Wallet.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
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
                    return Ok("User registered successfully. Confirmation codes sent.");
                }

                return BadRequest(result.Errors);
            }

            [HttpPost("verify-email")]
            public async Task<IActionResult> VerifyEmail(VerifyEmailModel model)
            {
                var isSuccess = await _userService.VerifyEmailAsync(model);
                if (isSuccess)
                {
                    return Ok("Email confirmed successfully.");
                }

                return BadRequest("Invalid or expired confirmation code.");
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginModel model)
            {
                var (user, requiresTwoFactor) = await _userService.LoginAsync(model);

                if (user == null)
                {
                    return Unauthorized("Invalid email or password.");
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
                    return Ok("Phone number verified successfully.");
                }

                return BadRequest("Invalid verification code.");
            }

            [HttpPost("uploadProfilePicture")]
            [Authorize]
            public async Task<IActionResult> UploadProfilePicture(IFormFile file)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                try
                {
                    await _userService.UploadProfilePictureAsync(userId, file);
                    return Ok(new { message = "Profile picture updated successfully" });
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
                    return Ok("User profile updated successfully. Please verify your new phone number.");
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
                    return NotFound("User not found.");
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
                    return NotFound("User not found.");
                }

                var is2faTokenValid = await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, model.Code);

                if (!is2faTokenValid)
                {
                    return BadRequest("Invalid verification code.");
                }

                await _twoFactorAuthService.EnableTwoFactorAuthenticationAsync(user);

                return Ok("2FA has been enabled.");
            }

            [HttpPost("login-2fa")]
            public async Task<IActionResult> Login2FA([FromBody] Login2FAModel model)
            {
                var user = await _userService.GetUserByIdAsync(model.UserId);
                if (user == null)
                {
                    return Unauthorized("Invalid user.");
                }

                var is2faTokenValid = await _userService.VerifyTwoFactorCodeAsync(user, model.Code);
                if (!is2faTokenValid)
                {
                    return Unauthorized("Invalid 2FA code.");
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
                    return NotFound("User not found.");
                }

                await _twoFactorAuthService.DisableTwoFactorAuthenticationAsync(user);
                return Ok("2FA has been disabled.");
            }
        }
    }
}
