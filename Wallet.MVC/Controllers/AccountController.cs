using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using Wallet.Data.Helpers.Contracts;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{

    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthManager _authManager;
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        public AccountController(IUserService userService, IAuthManager authManager, ITwoFactorAuthService twoFactorAuthService)
        {
            _userService = userService;
            _authManager = authManager;
            _twoFactorAuthService = twoFactorAuthService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (user, requiresTwoFactor) = await _userService.LoginAsync(model);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (requiresTwoFactor)
            {
                TempData["UserId"] = user.Id;
                return RedirectToAction("Verify2FA");
            }

            await SignInUserAsync(user);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Verify2FA()
        {
            return View(new Verify2FAModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FA(Verify2FAModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = TempData["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User session expired. Please log in again.");
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return RedirectToAction("Login");
            }

            var is2faTokenValid = await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, model.Code);
            if (!is2faTokenValid)
            {
                ModelState.AddModelError("", "Invalid verification code.");
                return View(model);
            }

            await SignInUserAsync(user);

            return RedirectToAction("Index", "Home");
        }

        // Helper method to sign in the user
        private async Task SignInUserAsync(AppUser user)
        {
            var token = await _authManager.GenerateJwtToken(user);

            // Set the JWT token in an HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use HTTPS in production
                Expires = DateTime.UtcNow.AddDays(1) // Match the JWT token expiry
            };

            Response.Cookies.Append("AuthToken", token, cookieOptions);

            // Sign in the user using Cookie Authentication
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userService.RegisterUserAsync(model);
                if (result.Succeeded)
                {
                    return RedirectToAction("VerifyEmail");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Catch specific exception and add a model error
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                // Catch any other exceptions
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("UserId");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail()
        {
            return View(new VerifyEmailModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isSuccess = await _userService.VerifyEmailAsync(model);
            if (isSuccess)
            {
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Invalid verification code or the code has expired.");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Enable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var qrCodeImage = await _twoFactorAuthService.GenerateQrCodeImageAsync(user);
            return File(qrCodeImage, "image/png");
        }

        [HttpPost]
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFactor(Verify2FAModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            var is2faTokenValid = await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, model.Code);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("", "Invalid verification code.");
                return View(model);
            }

            await _twoFactorAuthService.EnableTwoFactorAuthenticationAsync(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            await _twoFactorAuthService.DisableTwoFactorAuthenticationAsync(user);

            return RedirectToAction("Index", "Home");
        }
    }
}

