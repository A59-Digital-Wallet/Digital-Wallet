using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

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

        /// <summary>
        /// Creates a new transaction for the authenticated user.
        /// </summary>
        /// <param name="transactionRequest">The transaction request model containing transaction details.</param>
        /// <returns>A success message if the transaction is created successfully, or a verification request if needed.</returns>
        /// <response code="200">Transaction created successfully or verification required.</response>
        /// <response code="400">If the transaction data is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If an unexpected error occurs while processing the request.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Creates a new transaction for the authenticated user.")]
        [SwaggerResponse(200, "Transaction created successfully or verification required.", typeof(VerificationRequiredResponse))]
        [SwaggerResponse(400, "If the transaction data is invalid.")]
        [SwaggerResponse(401, "If the user is not authorized.")]
        [SwaggerResponse(500, "If an unexpected error occurs while processing the request.")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestModel transactionRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = Messages.Unauthorized });
            }

            try
            {
                await _transactionService.CreateTransactionAsync(transactionRequest, userId);
                return Ok(new { message = Messages.Controller.TransactionCreatedSuccessfully });
            }
            catch (VerificationRequiredException ex)
            {
                var response = new VerificationRequiredResponse
                {
                    TransactionToken = ex.TransactionToken,
                    Message = Messages.Controller.VerificationRequired
                };
                return Ok(response);
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

        /// <summary>
        /// Retrieves a paginated list of transactions for the authenticated user.
        /// </summary>
        /// <param name="filter">The filter criteria for transactions.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of transactions per page (default is 10).</param>
        /// <returns>A paginated list of transactions matching the filter criteria.</returns>
        /// <response code="200">Returns a list of transactions for the authenticated user.</response>
        /// <response code="400">If the page or page size is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If an unexpected error occurs while processing the request.</response>
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieves a paginated list of transactions for the authenticated user.")]
        [SwaggerResponse(200, "Returns a list of transactions for the authenticated user.")]
        [SwaggerResponse(400, "If the page or page size is invalid.")]
        [SwaggerResponse(401, "If the user is not authorized.")]
        [SwaggerResponse(500, "If an unexpected error occurs while processing the request.")]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionRequestFilter filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = Messages.Controller.PageOrPageSizeInvalid });
            }

            var userId = User.FindFirstValue(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = Messages.Unauthorized });
            }

            try
            {
                var transactions = await _transactionService.FilterTransactionsAsync(page, pageSize, filter, userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = Messages.OperationFailed, details = ex.Message });
            }
        }

        /// <summary>
        /// Verifies a transaction for the authenticated user.
        /// </summary>
        /// <param name="verifyRequest">The verification request model containing the token and verification code.</param>
        /// <returns>A success message if the transaction is verified successfully, or an error message if verification fails.</returns>
        /// <response code="200">If the transaction is verified successfully.</response>
        /// <response code="400">If the verification data is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If an unexpected error occurs while processing the request.</response>
        [HttpPost]
        [Route("verify-transaction")]
        [SwaggerOperation(Summary = "Verifies a transaction for the authenticated user.")]
        [SwaggerResponse(200, "If the transaction is verified successfully.")]
        [SwaggerResponse(400, "If the verification data is invalid.")]
        [SwaggerResponse(401, "If the user is not authorized.")]
        [SwaggerResponse(500, "If an unexpected error occurs while processing the request.")]
        public async Task<IActionResult> VerifyTransaction([FromBody] VerifyTransactionRequestModel verifyRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = Messages.Unauthorized });
            }

            try
            {
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

        /// <summary>
        /// Cancels a recurring transaction for the authenticated user.
        /// </summary>
        /// <param name="transactionId">The ID of the recurring transaction to cancel.</param>
        /// <returns>A success message if the recurring transaction is canceled successfully.</returns>
        /// <response code="200">If the recurring transaction is canceled successfully.</response>
        /// <response code="403">If the user is not authorized to cancel the transaction.</response>
        /// <response code="404">If the transaction is not found.</response>
        /// <response code="400">If the operation is invalid.</response>
        [HttpPost("cancel-recurring-transaction/{transactionId}")]
        [SwaggerOperation(Summary = "Cancels a recurring transaction for the authenticated user.")]
        [SwaggerResponse(200, "If the recurring transaction is canceled successfully.")]
        [SwaggerResponse(403, "If the user is not authorized to cancel the transaction.")]
        [SwaggerResponse(404, "If the transaction is not found.")]
        [SwaggerResponse(400, "If the operation is invalid.")]
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

        /// <summary>
        /// Adds a transaction to a specific category for the authenticated user.
        /// </summary>
        /// <param name="categoryId">The ID of the category to add the transaction to.</param>
        /// <param name="transactionId">The ID of the transaction to add to the category.</param>
        /// <returns>A success message if the transaction is added to the category successfully.</returns>
        /// <response code="200">If the transaction is added to the category successfully.</response>
        /// <response code="404">If the transaction or category is not found.</response>
        /// <response code="403">If the user is not authorized to add the transaction to the category.</response>
        [Authorize]
        [HttpPost("add-to-category")]
        [SwaggerOperation(Summary = "Adds a transaction to a specific category for the authenticated user.")]
        [SwaggerResponse(200, "If the transaction is added to the category successfully.")]
        [SwaggerResponse(404, "If the transaction or category is not found.")]
        [SwaggerResponse(403, "If the user is not authorized to add the transaction to the category.")]
        public async Task<IActionResult> AddTransactionToCategory(int categoryId, int transactionId)
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
