using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Wallet.Data.Models;
using Wallet.DTO.Response;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOverdraftSettingsService _overdraftSettingsService; 
        public AdminController(IUserService userService, IOverdraftSettingsService overdraftSettingsService)
        {
            _userService = userService;
            _overdraftSettingsService = overdraftSettingsService;
        }
        public async Task<IActionResult> AdminPanel()
        {
            var settingsResult = await _overdraftSettingsService.GetSettingsAsync();

            // Prepare the model with just the settings (and leave search-related properties empty)
            var viewModel = new AdminPanelViewModel()
            {
                CurrentOverdraftLimit = settingsResult.DefaultOverdraftLimit,
                CurrentConsecutiveNegativeMonths = settingsResult.DefaultConsecutiveNegativeMonths,
                CurrentInterestRate = settingsResult.DefaultInterestRate,

                UsersWithRoles = new List<UserWithRolesDto>(),
                PageNumber = 1,
                TotalPages = 0,
                SearchTerm = string.Empty
            };

            return View(viewModel);
        }
        public async Task<IActionResult> SearchUsersAsync(string? searchTerm, int page = 1)
        {
            int pageSize = 10;
            var result = await _userService.SearchUsersAsync(searchTerm, page, pageSize); 

            var viewModel = new UserSearchViewModel()
            {
                UsersWithRoles = result.Items,
                PageNumber = result.PageNumber,
                TotalPages = result.TotalCount,
                SearchTerm = searchTerm,
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManageRole(string userId, string action)
        {
            string searchTerm = Request.Form["searchTerm"];
            try
            {
                var result = await _userService.ManageRoleAsync(userId, action);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Action '{action}' was successfully performed on the user.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to perform '{action}' action on the user. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
            }

            // Redirect back to the search with the current search term and page number
            return RedirectToAction("SearchUsers", new { searchTerm });
        }


        [HttpPost]
        public async Task<IActionResult> SetInterestRateAsync(decimal newRate)
        {
            bool isSuccessful = await _overdraftSettingsService.SetInterestRateAsync(newRate);
            if (isSuccessful)
            {
                TempData["SuccessMessage"] = $"Default interest rate updated to {newRate}%";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update the default interest rate.";
            }

            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public async Task<IActionResult> SetOverdraftLimitAsync(decimal newLimit)
        {
            bool isSuccessful = await _overdraftSettingsService.SetOverdraftLimitAsync(newLimit);
            if (isSuccessful)
            {
                TempData["SuccessMessage"] = $"Default overdraft limit updated to {newLimit}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update the default overdraft limit.";
            }

            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public async Task<IActionResult> SetConsecutiveNegativeMonthsAsync(int months)
        {
            bool isSuccessful = await _overdraftSettingsService.SetConsecutiveNegativeMonthsAsync(months);
            if (isSuccessful)
            {
                TempData["SuccessMessage"] = $"Default consecutive negative months updated to {months} months.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update the default consecutive negative months.";
            }

            return RedirectToAction("AdminPanel");
        }
    }
}
