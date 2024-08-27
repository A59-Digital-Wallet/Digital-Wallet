using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Models;
using Wallet.Common.Exceptions;
using Wallet.DTO.Response;
using Wallet.Common.Helpers;

namespace Wallet.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestModel transactionRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = Messages.Unauthorized }); //"User ID is missing from the token."
            }

            try
            {
                await _transactionService.CreateTransactionAsync(transactionRequest, userId);
                return Ok(new { message = Messages.Controller.TransactionCreatedSuccessfully });
            }
            catch (VerificationRequiredException ex)
            {
                // Return a response indicating that verification is required
                var response = new VerificationRequiredResponse
                {
                    TransactionToken = ex.TransactionToken,
                    Message = Messages.Controller.VerificationRequired
                };
                return Ok(response); // Return an OK status with the verification information
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = Messages.OperationFailed, details = ex.Message }); //"An error occurred while processing the request."
            }
        }



        // GET: api/transactions
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionRequestFilter filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = Messages.Controller.PageOrPageSizeInvalid });
            }

            var userId = User.FindFirstValue(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = Messages.Unauthorized }); //"User ID is missing from the token."
            }

            try
            {
                var transactions = await _transactionService.FilterTransactionsAsync(page, pageSize, filter, userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = Messages.OperationFailed, details = ex.Message }); // "An error occurred while processing the request."
            }
        }
        [HttpPost]
        [Route("verify-transaction")]
        public async Task<IActionResult> VerifyTransaction([FromBody] VerifyTransactionRequestModel verifyRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = Messages.Unauthorized }); //"User ID is missing from the token."
            }

            try
            {
                // Attempt to verify the transaction
                bool result = await _transactionService.VerifyTransactionAsync(verifyRequest.Token, verifyRequest.VerificationCode);

                if (result)
                {
                    return Ok(new { message = Messages.Controller.TransactionVerifiedSuccessfully });
                }
                else
                {
                    return BadRequest(new { error = Messages.Controller.VerificationFailed });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = Messages.OperationFailed, details = ex.Message });
            }
        }


        [HttpGet("search-user")]
        [Authorize]
        public async Task<IActionResult> SearchUser(string searchTerm)
        {
            try
            {
                var userWithWallets = await _transactionService.SearchUserWithWalletsAsync(searchTerm);
                return Ok(userWithWallets);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = Messages.OperationFailed, details = ex.Message });
            }
        }

        [HttpPost("cancel-recurring-transaction/{transactionId}")]
        [Authorize]
        public async Task<IActionResult> CancelRecurringTransaction(int transactionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _transactionService.CancelRecurringTransactionAsync(transactionId, userId);
                return Ok(Messages.Controller.RecurringTransactionCancelledSuccessfully);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("add-to-category")]
        public async Task<IActionResult> AddTransactionToCategory (int categoryId, int transactionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _transactionService.AddTransactionToCategoryAsync(transactionId, categoryId, userId);
                return Ok(new { message = Messages.Controller.TransactionAddedToCategorySuccessfully });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
