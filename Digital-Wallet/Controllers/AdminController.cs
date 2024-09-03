using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations; // Import for Swagger annotations
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

        /// <summary>
        /// Get a paginated list of users with optional search term.
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter users.</param>
        /// <param name="page">Page number for pagination (default is 1).</param>
        /// <param name="pageSize">Number of users per page (default is 10).</param>
        /// <returns>List of users matching the search criteria.</returns>
        [HttpGet("users")]
        [SwaggerOperation(Summary = "Get a paginated list of users", Description = "Fetches a paginated list of users with an optional search term.")]
        [SwaggerResponse(200, "Returns a list of users.")]
        [SwaggerResponse(400, "Bad request.")]
        public async Task<IActionResult> GetUsers(string? searchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _userService.SearchUsersAsync(searchTerm, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get details of a user by their ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>User details.</returns>
        [HttpGet("users/{id}")]
        [SwaggerOperation(Summary = "Get user by ID", Description = "Fetches the details of a user by their ID.")]
        [SwaggerResponse(200, "Returns the user details.")]
        [SwaggerResponse(404, "User not found.")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(Messages.UserNotFound);
            }
            return Ok(user);
        }

        /// <summary>
        /// Manage a user's role.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="action">Action to perform (add or remove role).</param>
        /// <returns>Status of the operation.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Manage user role", Description = "Adds or removes a role from a user.")]
        [SwaggerResponse(200, "Role managed successfully.")]
        [SwaggerResponse(400, "Invalid request or action failed.")]
        [SwaggerResponse(404, "User not found.")]
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
                return StatusCode(500, Messages.OperationFailed);
            }
        }

        /// <summary>
        /// Set the default interest rate for overdraft settings.
        /// </summary>
        /// <param name="newRate">New interest rate.</param>
        /// <returns>Status of the operation.</returns>
        [HttpPut("default-interest-rate")]
        [SwaggerOperation(Summary = "Set default interest rate", Description = "Sets the default interest rate for overdraft settings.")]
        [SwaggerResponse(200, "Interest rate set successfully.")]
        [SwaggerResponse(400, "Failed to set interest rate.")]
        public async Task<IActionResult> SetInterestRate(decimal newRate)
        {
            bool isSuccessful = await _overdraftSettingsService.SetInterestRateAsync(newRate);
            if (isSuccessful)
            {
                return Ok(string.Format(Messages.Controller.InterestRateSuccessful, newRate));
            }
            return BadRequest();
        }

        /// <summary>
        /// Set the default overdraft limit.
        /// </summary>
        /// <param name="newLimit">New overdraft limit.</param>
        /// <returns>Status of the operation.</returns>
        [HttpPut("default-overdraft-limit")]
        [SwaggerOperation(Summary = "Set default overdraft limit", Description = "Sets the default overdraft limit.")]
        [SwaggerResponse(200, "Overdraft limit set successfully.")]
        [SwaggerResponse(400, "Failed to set overdraft limit.")]
        public async Task<IActionResult> SetOverdraftLimit(decimal newLimit)
        {
            bool isSuccessful = await _overdraftSettingsService.SetOverdraftLimitAsync(newLimit);
            if (isSuccessful)
            {
                return Ok(string.Format(Messages.Controller.OverdraftLimitSuccessful, newLimit));
            }
            return BadRequest();
        }

        /// <summary>
        /// Set the default number of consecutive negative months allowed.
        /// </summary>
        /// <param name="months">Number of months.</param>
        /// <returns>Status of the operation.</returns>
        [HttpPut("default-consecutive-negative-months")]
        [SwaggerOperation(Summary = "Set default consecutive negative months", Description = "Sets the default number of consecutive negative months allowed.")]
        [SwaggerResponse(200, "Consecutive negative months set successfully.")]
        [SwaggerResponse(400, "Failed to set consecutive negative months.")]
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
