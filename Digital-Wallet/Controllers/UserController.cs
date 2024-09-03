using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Wallet.Common.Exceptions;
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

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">The registration model containing user details.</param>
        /// <returns>A success message if the user is registered successfully.</returns>
        /// <response code="200">If the user is registered successfully.</response>
        /// <response code="400">If the registration data is invalid.</response>
        [HttpPost("add-user")]
        [SwaggerOperation(Summary = "Registers a new user.")]
        [SwaggerResponse(200, "If the user is registered successfully.")]
        [SwaggerResponse(400, "If the registration data is invalid.")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var result = await _userService.RegisterUserAsync(model);
            if (result.Succeeded)
            {
                return Ok(Messages.Controller.RegistrationSuccess);
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Verifies a user's email address.
        /// </summary>
        /// <param name="model">The verification model containing the email and confirmation code.</param>
        /// <returns>A success message if the email is verified successfully.</returns>
        /// <response code="200">If the email is verified successfully.</response>
        /// <response code="400">If the confirmation code is invalid or expired.</response>
        [HttpPost("verify-email")]
        [SwaggerOperation(Summary = "Verifies a user's email address.")]
        [SwaggerResponse(200, "If the email is verified successfully.")]
        [SwaggerResponse(400, "If the confirmation code is invalid or expired.")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailModel model)
        {
            var isSuccess = await _userService.VerifyEmailAsync(model);
            if (isSuccess)
            {
                return Ok(Messages.Controller.EmailConfirmationSuccess);
            }

            return BadRequest(Messages.Controller.InvalidOrExpiredConfirmationCode);
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="model">The login model containing email and password.</param>
        /// <returns>A JWT token if login is successful, or an indication that 2FA is required.</returns>
        /// <response code="200">If the login is successful or if 2FA is required.</response>
        /// <response code="401">If the email or password is invalid.</response>
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Logs in a user.")]
        [SwaggerResponse(200, "If the login is successful or if 2FA is required.")]
        [SwaggerResponse(401, "If the email or password is invalid.")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var (user, requiresTwoFactor) = await _userService.LoginAsync(model);

            if (user == null)
            {
                return Unauthorized(Messages.Controller.InvalidEmailOrPassword);
            }

            if (requiresTwoFactor)
            {
                return Ok(new { TwoFactorRequired = true, userId = user.Id });
            }

            var authToken = await _authManager.GenerateJwtToken(user);
            return Ok(new { token = authToken });
        }

        /// <summary>
        /// Verifies a user's phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to verify.</param>
        /// <param name="code">The verification code sent to the phone.</param>
        /// <returns>A success message if the phone number is verified successfully.</returns>
        /// <response code="200">If the phone number is verified successfully.</response>
        /// <response code="400">If the verification code is invalid.</response>
        [HttpPost("verify-phone")]
        [SwaggerOperation(Summary = "Verifies a user's phone number.")]
        [SwaggerResponse(200, "If the phone number is verified successfully.")]
        [SwaggerResponse(400, "If the verification code is invalid.")]
        public async Task<IActionResult> VerifyPhone(string phoneNumber, string code)
        {
            var isSuccess = await _userService.VerifyPhoneAsync(phoneNumber, code);
            if (isSuccess)
            {
                return Ok(Messages.Controller.PhoneNumberVerificationSuccess);
            }

            return BadRequest(Messages.Controller.InvalidVerificationCode);
        }

        /// <summary>
        /// Uploads a profile picture for the authenticated user.
        /// </summary>
        /// <param name="file">The profile picture file to upload.</param>
        /// <returns>A success message if the profile picture is uploaded successfully.</returns>
        /// <response code="200">If the profile picture is uploaded successfully.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="400">If the file is invalid.</response>
        /// <response code="500">If an unexpected error occurs while processing the request.</response>
        [HttpPost("uploadProfilePicture")]
        [Authorize]
        [SwaggerOperation(Summary = "Uploads a profile picture for the authenticated user.")]
        [SwaggerResponse(200, "If the profile picture is uploaded successfully.")]
        [SwaggerResponse(404, "If the user is not found.")]
        [SwaggerResponse(400, "If the file is invalid.")]
        [SwaggerResponse(500, "If an unexpected error occurs while processing the request.")]
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

        /// <summary>
        /// Updates the profile of the authenticated user.
        /// </summary>
        /// <param name="model">The update model containing new user profile details.</param>
        /// <returns>A success message if the profile is updated successfully.</returns>
        /// <response code="200">If the profile is updated successfully.</response>
        /// <response code="400">If the update data is invalid.</response>
        [HttpPost("update-profile")]
        [Authorize]
        [SwaggerOperation(Summary = "Updates the profile of the authenticated user.")]
        [SwaggerResponse(200, "If the profile is updated successfully.")]
        [SwaggerResponse(400, "If the update data is invalid.")]
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

        /// <summary>
        /// Enables 2-factor authentication for the authenticated user.
        /// </summary>
        /// <returns>A QR code image for configuring 2FA.</returns>
        /// <response code="200">Returns a QR code image for configuring 2FA.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpGet("enable-2fa")]
        [Authorize]
        [Produces("image/png")]
        [SwaggerOperation(Summary = "Enables 2-factor authentication for the authenticated user.")]
        [SwaggerResponse(200, "Returns a QR code image for configuring 2FA.")]
        [SwaggerResponse(404, "If the user is not found.")]
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

        /// <summary>
        /// Verifies the 2-factor authentication code for the authenticated user.
        /// </summary>
        /// <param name="model">The verification model containing the 2FA code.</param>
        /// <returns>A success message if the 2FA code is verified successfully.</returns>
        /// <response code="200">If the 2FA code is verified successfully.</response>
        /// <response code="400">If the 2FA code is invalid.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpPost("verify-2fa")]
        [Authorize]
        [SwaggerOperation(Summary = "Verifies the 2-factor authentication code for the authenticated user.")]
        [SwaggerResponse(200, "If the 2FA code is verified successfully.")]
        [SwaggerResponse(400, "If the 2FA code is invalid.")]
        [SwaggerResponse(404, "If the user is not found.")]
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

        /// <summary>
        /// Logs in a user using 2-factor authentication.
        /// </summary>
        /// <param name="model">The 2FA login model containing the user ID and 2FA code.</param>
        /// <returns>A JWT token if the 2FA login is successful.</returns>
        /// <response code="200">If the 2FA login is successful.</response>
        /// <response code="401">If the 2FA code is invalid or the user is not found.</response>
        [HttpPost("login-2fa")]
        [SwaggerOperation(Summary = "Logs in a user using 2-factor authentication.")]
        [SwaggerResponse(200, "If the 2FA login is successful.")]
        [SwaggerResponse(401, "If the 2FA code is invalid or the user is not found.")]
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
                return Unauthorized(Messages.Controller.InvalidVerificationCode);
            }

            var authToken = await _authManager.GenerateJwtToken(user);
            return Ok(new { token = authToken });
        }

        /// <summary>
        /// Disables 2-factor authentication for the authenticated user.
        /// </summary>
        /// <returns>A success message if 2FA is disabled successfully.</returns>
        /// <response code="200">If 2FA is disabled successfully.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpPost("disable-2fa")]
        [Authorize]
        [SwaggerOperation(Summary = "Disables 2-factor authentication for the authenticated user.")]
        [SwaggerResponse(200, "If 2FA is disabled successfully.")]
        [SwaggerResponse(404, "If the user is not found.")]
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
