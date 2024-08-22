using Microsoft.AspNetCore.Identity;
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
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly ICardService _cardService;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWalletFactory _walletFactory;

        public TransactionController(IWalletService walletService, ICardService cardService, ITransactionService transactionService, ICategoryService categoryService, UserManager<AppUser> userManager, IWalletFactory walletFactory)
        {
            _walletService = walletService;
            _cardService = cardService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _userManager = userManager;
            _walletFactory = walletFactory;
        }

        [HttpGet]
        public async Task<IActionResult> SelectWalletAndCard(string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            // Fetch the user's preferred wallet directly
            var user = await _userManager.FindByIdAsync(userId);
            var preferredWallet = await _walletService.GetWalletAsync((int)user.LastSelectedWalletId, userId);

            if (preferredWallet == null)
            {
                return RedirectToAction("Index", "Home"); // Or handle the case where the preferred wallet isn't set
            }

            var cards = await _cardService.GetCardsAsync(userId);

            var model = new WalletAndCardSelectionViewModel
            {
                TransactionType = type,
                SelectedWalletId = preferredWallet.Id, // Automatically use the preferred wallet
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
                    WalletId = model.SelectedWalletId, // This is automatically set in SelectWalletAndCard
                    Amount = model.Amount,
                    Description = model.Description,
                    TransactionType = model.TransactionType == "Deposit" ? TransactionType.Deposit : TransactionType.Withdraw,
                    CardId = int.Parse(model.SelectedCardId),
                    IsRecurring = model.IsRecurring,
                    RecurrenceInterval = model.IsRecurring ? model.RecurrenceInterval : null
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

        [HttpPost]
        public async Task<IActionResult> CancelRecurringTransaction(int transactionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _transactionService.CancelRecurringTransactionAsync(transactionId, userId);
                TempData["SuccessMessage"] = "Recurring transaction has been successfully canceled.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("TransactionHistory");
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
                        Id= t.Id,
                        Date = t.Date,
                        Amount = t.Amount,
                        Description = t.Description,
                        Type = t.TransactionType.ToString(),
                        Direction = t.Direction, // Direction determined by the service,
                         FromWallet = t.WalletName,  // Pass the From wallet name
                        ToWallet = t.RecepientWalledName,
                        IsRecurring = t.IsReccuring,
                        RecurrenceInterval = t.RecurrenceInterval,
                    }).ToList()
                }).ToList();

            var model = new TransactionHistoryViewModel
            {
                MonthlyTransactions = groupedTransactions,
                Filter = filter
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SearchUser(TransferViewModel model)
        {
            if (string.IsNullOrEmpty(model.RecipientUsernameOrEmail))
            {
                ModelState.AddModelError("RecipientUsernameOrEmail", "Please enter a username or email.");
                return View("InitiateTransfer", model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.UserData));
                var userWithWallets = await _transactionService.SearchUserWithWalletsAsync(model.RecipientUsernameOrEmail);

                if (userWithWallets == null)
                {
                    ModelState.AddModelError("RecipientUsernameOrEmail", "No user found with the given username or email.");
                    return View("InitiateTransfer", model);
                }

                // Set the selected recipient ID and the first wallet ID
                model.SelectedRecipientId = userWithWallets.UserId;
                var preferredWallet = userWithWallets.Wallets.FirstOrDefault(w => w.Currency == model.Currency)
                                     ?? userWithWallets.Wallets.First();

                model.ToWalletId = preferredWallet.WalletId;
                model.FromWalletId = (int)user.LastSelectedWalletId;
                // Pass the necessary information back to the view
                return View("InitiateTransfer", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while searching for the user: " + ex.Message);
                return View("InitiateTransfer", model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> InitiateTransfer()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var user = await _userManager.FindByIdAsync(userId);

            var categories = new List<SelectListItem>();
            try
            {
                var categoryList = await _categoryService.GetUserCategoriesAsync(userId, 1, int.MaxValue);
                categories = categoryList.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
            }
            catch (EntityNotFoundException)
            {
                ViewBag.NoCategoriesMessage = "No categories available";
            }

            var model = new TransferViewModel
            {
                FromWalletId = (int)user.LastSelectedWalletId,
                Categories = categories,
                // Initialize other fields to prevent null reference issues
                RecipientWallets = new List<SelectListItem>(),
                SelectedRecipientId = "",
                ToWalletId = 0
            };

            return View(model);
        }





        [HttpPost]
        public async Task<IActionResult> InitiateTransfer(TransferViewModel model)
        {
            try
            {
                if (model.SelectedRecipientId == null || model.ToWalletId == 0)
                {
                    ModelState.AddModelError("RecipientUsernameOrEmail", "Please search and select a valid recipient.");
                    return View(model);
                }

                // Proceed to process the transfer
                return RedirectToAction("ProcessTransfer", new
                {
                    FromWalletId = model.FromWalletId,
                    ToWalletId = model.ToWalletId,
                    Amount = model.Amount,
                    Description = model.Description,
                    SelectedCategoryId = model.SelectedCategoryId,
                    IsRecurring = model.IsRecurring,
                    RecurrenceInterval = model.RecurrenceInterval
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while initiating the transfer: " + ex.Message);
                return View(model);
            }
        }



        [HttpPost]
        public async Task<IActionResult> ProcessTransfer(TransferViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                if (model.ToWalletId == 0)
                {
                    ModelState.AddModelError(string.Empty, "Recipient wallet is not selected. Please ensure you have selected a recipient.");
                    return View("InitiateTransfer", model);
                }

                var transactionRequest = new TransactionRequestModel
                {
                    WalletId = model.FromWalletId,
                    Amount = model.Amount,
                    Description = model.Description,
                    TransactionType = TransactionType.Transfer,
                    RecepientWalletId = model.ToWalletId,
                    CategoryId = model.SelectedCategoryId,
                    IsRecurring = model.IsRecurring,
                    RecurrenceInterval = model.RecurrenceInterval
                };

                await _transactionService.CreateTransactionAsync(transactionRequest, userId);

                return RedirectToAction("TransactionHistory");
            }
            catch (VerificationRequiredException ex)
            {
                return RedirectToAction("ConfirmTransaction", "Transaction", new { token = ex.TransactionToken });
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
