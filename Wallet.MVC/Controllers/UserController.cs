using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.DTO.Request;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;

namespace Wallet.MVC.Controllers
{
    [Authorize]
  
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public UserController(IUserService userService, IAccountService accountService, ITwoFactorAuthService twoFactorAuthService)
        {
            _userService = userService;
            _accountService = accountService;
            _twoFactorAuthService = twoFactorAuthService;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var user = await _userService.GetUserByIdAsync(userId);

            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsTwoFactorEnabled = await _accountService.IsTwoFactorEnabledAsync(user),
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                ProfilePictureUrl = user.ProfilePictureURL
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var updateUserModel = new UpdateUserModel
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userService.UpdateUserProfileAsync(updateUserModel, userId);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Profile", model);
            }
   

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var user = await _userService.GetUserByIdAsync(userId);

            var qrCodeImage = await _twoFactorAuthService.GenerateQrCodeImageAsync(user);
            ViewBag.QrCodeImage = Convert.ToBase64String(qrCodeImage);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication(string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var user = await _userService.GetUserByIdAsync(userId);

            if (await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, code))
            {
                await _twoFactorAuthService.EnableTwoFactorAuthenticationAsync(user);
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, "Invalid code. Please try again.");
            var qrCodeImage = await _twoFactorAuthService.GenerateQrCodeImageAsync(user);
            ViewBag.QrCodeImage = Convert.ToBase64String(qrCodeImage);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(userId);

            await _twoFactorAuthService.DisableTwoFactorAuthenticationAsync(user);

            return RedirectToAction("Profile");
        }

    }
}
