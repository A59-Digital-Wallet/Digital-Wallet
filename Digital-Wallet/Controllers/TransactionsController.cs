﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Models;

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
                return Unauthorized(new { error = "User ID is missing from the token." });
            }

            try
            {
                // Assuming CreateTransactionAsync can accept the user ID as part of the transaction creation
                await _transactionService.CreateTransactionAsync(transactionRequest, userId);
                return Ok(new { message = "Transaction created successfully." });
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
                return StatusCode(500, new { error = "An error occurred while processing the request.", details = ex.Message });
            }
        }

        // GET: api/transactions
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionRequestFilter filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = "Page and pageSize must be greater than 0." });
            }

            var userId = User.FindFirstValue(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User ID is missing from the token." });
            }

            try
            {
                var transactions = await _transactionService.FilterTransactionsAsync(page, pageSize, filter, userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing the request.", details = ex.Message });
            }
        }
    }
}
