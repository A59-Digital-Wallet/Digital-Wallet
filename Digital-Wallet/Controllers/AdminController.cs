using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public AdminController(IUserService userService, UserManager<AppUser> userManager)
        {
            _userService = userService;
            this.userManager = userManager;
        }

        // GET: api/admin/users
        [HttpGet("users")]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers(string? searchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _userService.SearchUsersAsync(searchTerm, page, pageSize);
            return Ok(result);
        }

        // GET: api/admin/users/{id}
        [HttpGet("users/{id}")]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }

        [HttpPost]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRole(string userId, string action)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            IdentityResult result = null;

            switch (action.ToLower())
            {
                case "block":
                    if (await this.userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return BadRequest("Admin users cannot be blocked.");
                    }
                    result = await AssignRole(user, "Blocked");
                    break;
                case "unblock":
                    result = await UnassignRole(user, "Blocked");
                    break;
                case "makeadmin":
                    result = await AssignRole(user, "Admin");
                    break;
                default:
                    return BadRequest("Invalid action. Use 'block', 'unblock', or 'makeadmin'.");
            }

            if (result.Succeeded)
            {
                return Ok($"Action '{action}' was successfully performed on user {user.UserName}.");
            }

            return BadRequest($"Failed to perform '{action}' action on user {user.UserName}.");
        }

        private async Task<IdentityResult> AssignRole(AppUser user, string role)
        {
            if (await this.userManager.IsInRoleAsync(user, role))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User is already in the '{role}' role."
                });
            }

            return await this.userManager.AddToRoleAsync(user, role);
        }

        private async Task<IdentityResult> UnassignRole(AppUser user, string role)
        {
            if (!await this.userManager.IsInRoleAsync(user, role))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User is not in the '{role}' role."
                });
            }

            return await this.userManager.RemoveFromRoleAsync(user, role);
        }
    }

}
