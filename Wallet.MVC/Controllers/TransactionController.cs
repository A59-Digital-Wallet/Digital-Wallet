using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;

namespace Wallet.MVC.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly ICardService _cardService;
        private readonly ITransactionService _transactionService;

        public TransactionController(IWalletService walletService, ICardService cardService, ITransactionService transactionService)
        {
            _walletService = walletService;
            _cardService = cardService;
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> SelectWalletAndCard(string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallets = await _walletService.GetUserWalletsAsync(userId);
            var cards = await _cardService.GetCardsAsync(userId);

            var model = new WalletAndCardSelectionViewModel
            {
                TransactionType = type,
                Wallets = wallets.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.Name} - {w.Balance.ToString("C")}"
                }).ToList(),
                Cards = cards.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.CardNumber} - {c.CardHolderName} (Exp: {c.ExpiryDate:MM/yy})"
                }).ToList(),
                
                
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTransaction(WalletAndCardSelectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                var transactionRequest = new TransactionRequestModel
                {
                    WalletId = model.SelectedWalletId,
                    Amount = model.Amount,
                    Description = model.Description,
                    TransactionType = model.TransactionType == "Deposit" ? TransactionType.Deposit : TransactionType.Withdraw,
                    CardId = int.Parse(model.SelectedCardId)
                };

                try
                {
                    await _transactionService.CreateTransactionAsync(transactionRequest, userId);
                    return RedirectToAction("Index", "Home");
                }
                catch (VerificationRequiredException ex)
                {
                    var transactionConfig = new TransactionConfirmationViewModel
                    {
                        WalletId = model.SelectedWalletId,
                        CardId = int.Parse(model.SelectedCardId),
                        Amount = model.Amount,
                        Description = model.Description,
                        TransactionType = model.TransactionType,
                        TransactionToken = ex.TransactionToken,
                    };
                    // Redirect to the confirmation page
                    return RedirectToAction("ConfirmTransaction", transactionConfig);
                    
                }
            }

            return View("SelectWalletAndCard", model);
        }

        // GET method to show the confirmation view
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
                    TransactionType = model.TransactionType == "Deposit" ? TransactionType.Deposit : TransactionType.Withdraw,
                    CardId = model.CardId,
                    Token = model.TransactionToken // Pass the token to finalize the transaction
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

        public async Task<IActionResult> TransactionHistory(TransactionRequestFilter filter, int page = 1, int pageSize = 100)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            var transactions = await _transactionService.FilterTransactionsAsync(page, pageSize, filter, userId);

            var groupedTransactions = transactions
                .GroupBy(t => t.Date.ToString("MMMM yyyy"))
                .Select(g => new MonthlyTransactionViewModel
                {
                    MonthYear = g.Key,
                    Transactions = g.Select(t => new TransactionViewModel
                    {
                        Date = t.Date,
                        Amount = t.Amount,
                        Description = t.Description,
                        Type = t.TransactionType.ToString(),
                        Direction = t.Direction // Direction determined by the service
                    }).ToList()
                }).ToList();

            var model = new TransactionHistoryViewModel
            {
                MonthlyTransactions = groupedTransactions,
                Filter = filter
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> InitiateTransfer()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            // Fetch user wallets to populate the dropdown
            var wallets = await _walletService.GetUserWalletsAsync(userId);

            var model = new TransferViewModel
            {
                Wallets = wallets.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.Name} - {w.Balance.ToString("C")}"
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> InitiateTransfer(TransferViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            if (ModelState.IsValid && !string.IsNullOrEmpty(model.RecipientUsername))
            {
                try
                {
                    var userWithWallets = await _transactionService.SearchUserWithWalletsAsync(model.RecipientUsername);
                    model.RecipientWallets = userWithWallets.Wallets.Select(w => new SelectListItem
                    {
                        Value = w.WalletId.ToString(),
                        Text = $"{w.Currency} - {w.Balance:N2}"
                    }).ToList();
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("RecipientUsername", ex.Message);
                }
            }

            // Reload the user's wallets for the dropdown
            var wallets = await _walletService.GetUserWalletsAsync(userId);
            model.Wallets = wallets.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = $"{w.Name} - {w.Balance.ToString("C")}"
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTransfer(TransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                var wallets = await _walletService.GetUserWalletsAsync(userId);
                model.Wallets = wallets.Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.Name} - {w.Balance.ToString("C")}"
                }).ToList();
                return View("InitiateTransfer", model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                // Create a transaction request model for the transfer
                var transactionRequest = new TransactionRequestModel
                {
                    WalletId = model.FromWalletId,
                    Amount = model.Amount,
                    Description = $"Transfer to Wallet ID {model.ToWalletId}",
                    TransactionType = TransactionType.Transfer,
                    RecepientWalletId = model.ToWalletId,
                    Token = null // Initially null; will be generated if needed
                };

                // Execute the transaction
                await _transactionService.CreateTransactionAsync(transactionRequest, userId);

                return RedirectToAction("TransactionHistory");
            }
            catch (VerificationRequiredException ex)
            {
                // Handle the case where a verification is required for high-value transactions
                return RedirectToAction("ConfirmTransaction", "Transaction", new { token = ex });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("InitiateTransfer", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchRecipientWallets(string searchTerm)
        {
            try
            {
                var userWithWallets = await _transactionService.SearchUserWithWalletsAsync(searchTerm);
                return Json(userWithWallets);
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

      



    }

}
