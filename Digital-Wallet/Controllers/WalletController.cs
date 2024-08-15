using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.DTO.Request;
using Wallet.Services;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.API.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] UserWalletRequest wallet)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            await _walletService.CreateWallet(wallet, userId);
            return Ok(new { message = "Wallet created successfully." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWallet(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                var wallet = await _walletService.GetWalletAsync(id, userId);
                return Ok(wallet);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException)
            {
                return NotFound(new { message = "Wallet not found." });
            }
        }

        [HttpPost("{id}/add-member")]
        
        public async Task<IActionResult> AddMemberToJointWallet(int id, [FromBody] ManagePermissionsModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                await _walletService.AddMemberToJointWalletAsync(id, model.UserId, model.CanSpend, model.CanAddFunds, userId);
                return Ok(new { message = "Member added successfully to the joint wallet." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/remove-member")]
        
        public async Task<IActionResult> RemoveMemberFromJointWallet(int id, [FromBody] string userIdToRemove)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                await _walletService.RemoveMemberFromJointWalletAsync(id, userIdToRemove, userId);
                return Ok(new { message = "Member removed successfully from the joint wallet." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{walletId}/toggle-overdraft")]
        public async Task<IActionResult> ToggleOverdraft(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _walletService.ToggleOverdraftAsync(walletId, userId);
                return Ok(new { success = true, message = "Overdraft setting updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
