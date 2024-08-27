using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Services.Contracts;

namespace Digital_Wallet.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> userManager;
        private readonly IOverdraftSettingsService _overdraftSettingsService;
        public AdminController(IUserService userService, UserManager<AppUser> userManager, IOverdraftSettingsService overdraftSettingsService)
        {
            _userService = userService;
            this.userManager = userManager;
            _overdraftSettingsService = overdraftSettingsService;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(string? searchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _userService.SearchUsersAsync(searchTerm, page, pageSize);
            return Ok(result);
        }

        // GET: api/admin/users/{id}
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(Messages.UserNotFound);
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> ManageRole(string userId, string action)
        {
            try
            {
                var result = await _userService.ManageRoleAsync(userId, action);

                if (result.Succeeded)
                {
                    return Ok(string.Format(Messages.Controller.ActionSuccessful, action));
                }

                return BadRequest($"Failed to perform '{action}' action on the user. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for simplicity)
                return StatusCode(500, Messages.OperationFailed); //"An unexpected error occurred."
            }
        }

        [HttpPut("default-interest-rate")]
        public async Task<IActionResult> SetInterestRate(decimal newRate)
        {
            bool isSuccessful = await _overdraftSettingsService.SetInterestRateAsync(newRate);
            if (isSuccessful)
            {
                return Ok(string.Format(Messages.Controller.InterestRateSuccessful, newRate));
            }
            return BadRequest();
        }

        [HttpPut("default-overdraft-limit")]
        public async Task<IActionResult> SetOverdraftLimit(decimal newLimit)
        {
            bool isSuccessful = await _overdraftSettingsService.SetOverdraftLimitAsync(newLimit);
            if (isSuccessful)
            {
                return Ok(string.Format(Messages.Controller.OverdraftLimitSuccessful, newLimit));
            }
            return BadRequest();
        }

        [HttpPut("default-consecutive-negative-months")]
        public async Task<IActionResult> SetConsecutiveNegativeMonths(int months)
        {
            bool isSuccessful = await _overdraftSettingsService.SetConsecutiveNegativeMonthsAsync(months);
            if (isSuccessful)
            {
                return Ok(string.Format(Messages.Controller.NegativeMonthsSuccessful, months));
            }
            return BadRequest(Messages.Controller.NegativeMonthsFailed);
        }
    }

}
