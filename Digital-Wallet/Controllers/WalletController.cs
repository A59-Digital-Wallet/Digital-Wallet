using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Wallet.Common.Helpers;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;

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

        /// <summary>
        /// Creates a new wallet for the authenticated user.
        /// </summary>
        /// <param name="wallet">The wallet details for the new wallet.</param>
        /// <returns>A success message if the wallet is created successfully.</returns>
        /// <response code="200">If the wallet is created successfully.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new wallet for the authenticated user.")]
        [SwaggerResponse(200, "If the wallet is created successfully.")]
        public async Task<IActionResult> CreateWallet([FromBody] UserWalletRequest wallet)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            await _walletService.CreateWallet(wallet, userId);
            return Ok(new { message = Messages.Controller.WalletCreatedSuccessfully });
        }

        /// <summary>
        /// Retrieves the details of a specific wallet by its ID for the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the wallet to retrieve.</param>
        /// <returns>The wallet details if found and authorized; otherwise, a 403 or 404 error.</returns>
        /// <response code="200">Returns the wallet details if the wallet exists and belongs to the user.</response>
        /// <response code="403">If the user is not authorized to access the wallet.</response>
        /// <response code="404">If the wallet is not found.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Retrieves the details of a specific wallet by its ID for the authenticated user.")]
        [SwaggerResponse(200, "Returns the wallet details if the wallet exists and belongs to the user.")]
        [SwaggerResponse(403, "If the user is not authorized to access the wallet.")]
        [SwaggerResponse(404, "If the wallet is not found.")]
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
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Adds a member to a joint wallet.
        /// </summary>
        /// <param name="id">The ID of the wallet.</param>
        /// <param name="model">The permissions model for adding the member to the wallet.</param>
        /// <returns>A success message if the member is added successfully.</returns>
        /// <response code="200">If the member is added successfully.</response>
        /// <response code="400">If the input data is invalid.</response>
        [HttpPost("{id}/add-member")]
        [SwaggerOperation(Summary = "Adds a member to a joint wallet.")]
        [SwaggerResponse(200, "If the member is added successfully.")]
        [SwaggerResponse(400, "If the input data is invalid.")]
        public async Task<IActionResult> AddMemberToJointWallet(int id, [FromBody] ManagePermissionsModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                await _walletService.AddMemberToJointWalletAsync(id, model.UserId, model.CanSpend, model.CanAddFunds, userId);
                return Ok(new { message = Messages.Controller.MemberAddedToWalletSuccess });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Removes a member from a joint wallet.
        /// </summary>
        /// <param name="id">The ID of the wallet.</param>
        /// <param name="userIdToRemove">The user ID of the member to remove.</param>
        /// <returns>A success message if the member is removed successfully.</returns>
        /// <response code="200">If the member is removed successfully.</response>
        /// <response code="400">If the operation is invalid.</response>
        [HttpPost("{id}/remove-member")]
        [SwaggerOperation(Summary = "Removes a member from a joint wallet.")]
        [SwaggerResponse(200, "If the member is removed successfully.")]
        [SwaggerResponse(400, "If the operation is invalid.")]
        public async Task<IActionResult> RemoveMemberFromJointWallet(int id, [FromBody] string userIdToRemove)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                await _walletService.RemoveMemberFromJointWalletAsync(id, userIdToRemove, userId);
                return Ok(new { message = Messages.Controller.MemberRemovedFromWalletSuccess });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Toggles overdraft settings for a specific wallet.
        /// </summary>
        /// <param name="walletId">The ID of the wallet to toggle overdraft for.</param>
        /// <returns>A success message if the overdraft is toggled successfully.</returns>
        /// <response code="200">If the overdraft is toggled successfully.</response>
        /// <response code="400">If the operation is invalid.</response>
        [HttpPost("{walletId}/toggle-overdraft")]
        [SwaggerOperation(Summary = "Toggles overdraft settings for a specific wallet.")]
        [SwaggerResponse(200, "If the overdraft is toggled successfully.")]
        [SwaggerResponse(400, "If the operation is invalid.")]
        public async Task<IActionResult> ToggleOverdraft(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _walletService.ToggleOverdraftAsync(walletId, userId);
                return Ok(new { success = true, message = Messages.Controller.OverdraftUpdatedSuccessfully });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
