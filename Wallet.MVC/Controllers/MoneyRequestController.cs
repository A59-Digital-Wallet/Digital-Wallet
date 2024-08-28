using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;

namespace Wallet.MVC.Controllers
{

    public class MoneyRequestController : Controller
    {
        private readonly IMoneyRequestService _moneyRequestService;
        private readonly ITransactionService _transactionService;

        public MoneyRequestController(IMoneyRequestService moneyRequestService, ITransactionService transactionService)
        {
            _moneyRequestService = moneyRequestService;
            _transactionService = transactionService;
        }

        // Show the form to create a new money request
        [HttpGet]
        public IActionResult CreateRequest(string recipientId)
        {
            var model = new MoneyRequestCreateDTO
            {
                RecipientId = recipientId // Pass the recipient ID to the view
            };

            return View(model);
        }


        // Handle the form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(MoneyRequestCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var requesterId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _moneyRequestService.CreateMoneyRequestAsync(model, requesterId);
                TempData["SuccessMessage"] = "Money request sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View(model); // Return the same view with error message if something goes wrong
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ConfirmTransaction(TransactionConfirmationViewModel model)
        {
            return View(model);
        }

        // POST method to process the transaction after confirmation
        [HttpPost]
        public async Task<IActionResult> ProcessConfirmTransaction(TransactionConfirmationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                var transactionRequest = new TransactionRequestModel
                {
                    WalletId = model.WalletId,
                    Amount = model.Amount,
                    Description = model.Description,
                    TransactionType = TransactionType.Transfer,
                    CardId = model.CardId,
                    RecepientWalletId = model.RecipinetWalletId,
                    Token = model.TransactionToken, // Pass the token to finalize the transaction
                };

                // Verify the code and complete the transaction
                await _transactionService.CreateTransactionAsync(transactionRequest, userId, model.VerificationCode);

                return RedirectToAction("Index", "Home");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            // If verification fails, redisplay the form with an error message
            return View("ConfirmTransaction", model);
        }


        [HttpGet]
        public async Task<IActionResult> ViewReceivedRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var requests = await _moneyRequestService.GetReceivedRequestsAsync(userId);
            return View(requests); // Make sure you have a corresponding view to display these requests
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {

                await _moneyRequestService.ApproveMoneyRequestAsync(requestId, userId);
                return RedirectToAction("Index", "Home");
            }
            catch (VerificationRequiredException ex)
            {
                var transactionConfig = new TransactionConfirmationViewModel
                {
                    WalletId = ex.WalletId.GetValueOrDefault(),  // Use GetValueOrDefault for nullable int
                    Amount = ex.Amount.GetValueOrDefault(),
                    Description = ex.Description ?? $"Money Request approval for request ID {requestId}",
                    TransactionType = TransactionType.Transfer.ToString(),
                    TransactionToken = ex.TransactionToken,
                    RecipinetWalletId = ex.RecipientWallet
                };


                return RedirectToAction("ConfirmTransaction", "MoneyRequest", transactionConfig);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            try
            {
                await _moneyRequestService.UpdateMoneyRequestStatusAsync(requestId, RequestStatus.Declined);
                TempData["SuccessMessage"] = "Money request rejected.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
