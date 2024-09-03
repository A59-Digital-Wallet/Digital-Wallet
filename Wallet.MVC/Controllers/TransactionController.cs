using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{
    [Authorize]

    public class TransactionController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly ICardService _cardService;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWalletFactory _walletFactory;
        private readonly IContactService _contactService;

        public TransactionController(IWalletService walletService, ICardService cardService, ITransactionService transactionService, ICategoryService categoryService, UserManager<AppUser> userManager, IWalletFactory walletFactory, IContactService contactService)
        {
            _walletService = walletService;
            _cardService = cardService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            _userManager = userManager;
            _walletFactory = walletFactory;
            _contactService = contactService;
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
                return RedirectToAction("Index", "Dashboard"); // Or handle the case where the preferred wallet isn't set
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTransaction(WalletAndCardSelectionViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            // Check if the description is empty and add a model error
            if (string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError("Description", "Description is required.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate Cards list if ModelState is not valid
                var cards = await _cardService.GetCardsAsync(userId);
                model.Cards = cards.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.CardNumber} - {c.CardHolderName} (Exp: {c.ExpiryDate:MM/yy})"
                }).ToList();

                return View("SelectWalletAndCard", model); // Return view with repopulated data
            }

            var transactionRequest = new TransactionRequestModel
            {
                WalletId = model.SelectedWalletId, // This is automatically set in SelectWalletAndCard
                Amount = model.Amount,
                Description = model.Description,
                TransactionType = Enum.TryParse<TransactionType>(model.TransactionType, true, out var transaction) ? transaction : TransactionType.None,
                CardId = int.Parse(model.SelectedCardId),
                IsRecurring = model.IsRecurring,
                RecurrenceInterval = model.IsRecurring ? model.RecurrenceInterval : null
            };

            var transactionConfig = new TransactionConfirmationViewModel
            {
                WalletId = model.SelectedWalletId,
                CardId = int.Parse(model.SelectedCardId),
                Amount = model.Amount,
                Description = model.Description,
                TransactionType = model.TransactionType,
            };

            try
            {
                // Process the transaction logic here

                return RedirectToAction("ConfirmTransaction", transactionConfig);
            }
            catch (VerificationRequiredException ex)
            {
                transactionConfig.TransactionToken = ex.TransactionToken;
                transactionConfig.RequiresCode = true;
                return RedirectToAction("ConfirmTransaction", transactionConfig);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                // Check if a verification code is required and if it was provided
                if (model.RequiresCode && string.IsNullOrWhiteSpace(model.VerificationCode))
                {
                    ModelState.AddModelError(string.Empty, "Verification code is required for high-value transactions.");
                    return View("ConfirmTransaction", model);
                }

                var transactionRequest = new TransactionRequestModel
                {
                    WalletId = model.WalletId,
                    Amount = model.Amount,
                    Description = model.Description,
                    TransactionType = Enum.TryParse<TransactionType>(model.TransactionType, true, out var transaction) ? transaction : TransactionType.None,
                    CardId = model.CardId,
                    Token = model.TransactionToken,
                    RecepientWalletId = model.RecipinetWalletId,
                    CategoryId = model.CategoryId,
                };

                await _transactionService.CreateTransactionAsync(transactionRequest, userId, model.VerificationCode);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (VerificationRequiredException ex)
            {

                model.TransactionToken = ex.TransactionToken;
                model.RequiresCode = true;
                // Redirect to the confirmation page
                return RedirectToAction("ConfirmTransaction", model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View("ConfirmTransaction", model);
        }
        public async Task<IActionResult> TransactionHistory(TransactionRequestFilter filter, int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallets = await _walletService.GetUserWalletsAsync(userId);

            // Get paginated transactions and total count from the service
            var (transactions, totalCount) = await _transactionService.FilterTransactionsAsync(page, pageSize, filter, userId);

            // Group transactions by month and year
            var groupedTransactions = transactions
                .GroupBy(t => t.Date.ToString("MMMM yyyy"))
                .Select(g => new MonthlyTransactionViewModel
                {
                    MonthYear = g.Key,
                    Transactions = g.Select(t => new TransactionViewModel
                    {
                        Id = t.Id,
                        Date = t.Date,
                        Amount = t.Amount,
                        Description = t.Description,
                        Type = t.TransactionType.ToString(),
                        Direction = t.Direction,
                        FromWallet = t.WalletName,
                        ToWallet = t.RecepientWalledName,
                        IsRecurring = t.IsReccuring,
                        RecurrenceInterval = t.RecurrenceInterval,
                        OriginalAmount = t.OriginalAmount,
                        OriginalCurrency = t.OriginalCurrency,
                        SentCurrency = t.SentCurrency,
                        CurrencyCulture = CurrencyHelper.GetCurrencyCulture(t.OriginalCurrency),
                        CurrencyCultureSent = CurrencyHelper.GetCurrencyCulture(t.SentCurrency),
                    }).ToList()
                }).ToList();

            // Create the view model
            var model = new TransactionHistoryViewModel
            {
                MonthlyTransactions = groupedTransactions,
                Filter = filter,  // Keep the filters in the view model
                Wallets = wallets.Select(w => new WalletViewModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    Currency = w.Currency.ToString(),
                }).ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageSize = pageSize
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> InitiateTransfer(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var user = await _userManager.FindByIdAsync(userId);

            // Retrieve the user's last selected wallet to determine the currency
            var fromWallet = await _walletService.GetWalletAsync((int)user.LastSelectedWalletId, userId);
            var fromCurrency = fromWallet.Currency;

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

            // Retrieve recipient's wallets
            var recipientWallets = await _walletService.GetUserWalletsAsync(contactId);

            // Find a wallet with the same currency as the sender's wallet
            var matchingWallet = recipientWallets.FirstOrDefault(w => w.Currency == fromCurrency);

            // If no matching wallet found, use the recipient's last selected wallet
            var recipientWallet = matchingWallet ?? recipientWallets.FirstOrDefault(w => w.Id == w.Owner.LastSelectedWalletId);

            var model = new TransferViewModel
            {
                FromWalletId = fromWallet.Id,
                ToWalletId = recipientWallet?.Id ?? 0, // Set the ToWalletId based on the logic
                ContactId = contactId,
                Categories = categories,

            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ContactHistory(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            // Get all wallet IDs associated with the current user
            var userWallets = await _walletService.GetUserWalletsAsync(userId);
            var userWalletIds = userWallets.Select(w => w.Id).ToList();

            // Get the transaction history with the contact
            var transactions = await _transactionService.GetTransactionHistoryContactAsync(userId, contactId);

            var model = new ContactHistoryViewModel
            {
                ContactId = contactId,
                Transactions = transactions,
                UserWalletIds = userWalletIds,

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

                var transactionConfig = new TransactionConfirmationViewModel
                {
                    WalletId = model.FromWalletId,

                    Amount = model.Amount,
                    Description = model.Description,
                    TransactionType = TransactionType.Transfer.ToString(),
                    RecipinetWalletId = model.ToWalletId,
                    CategoryId = model.SelectedCategoryId

                };

                // Create the transaction
                //  await _transactionService.CreateTransactionAsync(transactionRequest, userId);

                // Automatically add the sending account to the recipient’s contacts

                var existingContact = await _contactService.GetContactAsync(model.ContactId, userId);
                if (existingContact == null)
                {
                    // If not, add the sending account to the recipient’s contacts
                    await _contactService.AddContactAsync(model.ContactId, userId);
                }

                return RedirectToAction("ConfirmTransaction", transactionConfig);
            }
            catch (VerificationRequiredException ex)
            {
                var transactionConfig = new TransactionConfirmationViewModel
                {
                    WalletId = ex.WalletId.GetValueOrDefault(),  // Use GetValueOrDefault for nullable int
                    Amount = ex.Amount.GetValueOrDefault(),
                    Description = ex.Description,
                    TransactionType = TransactionType.Transfer.ToString(),
                    TransactionToken = ex.TransactionToken,
                    RecipinetWalletId = ex.RecipientWallet
                };


                return RedirectToAction("ConfirmTransaction", transactionConfig);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("InitiateTransfer", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransferBetweenWallets()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallets = await _walletService.GetUserWalletsAsync(userId);
            var user = await _userManager.FindByIdAsync(userId);
            var model = new TransferViewModel
            {
                RecipientWallets = wallets.Where(w => w.Id != user.LastSelectedWalletId).Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.Name} - {w.Balance} {w.Currency}"
                }).ToList(),
                FromWalletId = (int)user.LastSelectedWalletId,
                SelectedRecipientId = userId

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTransferBetweenWallets(TransferViewModel model)
        {

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                var transactionRequest = new TransactionRequestModel
                {

                    Amount = model.Amount,
                    Description = model.Description,
                    TransactionType = TransactionType.Transfer,
                    RecepientWalletId = model.ToWalletId,
                    IsRecurring = model.IsRecurring,
                    RecurrenceInterval = model.IsRecurring ? model.RecurrenceInterval : null,
                    WalletId = model.FromWalletId
                };

                try
                {
                    await _transactionService.CreateTransactionAsync(transactionRequest, userId);
                    return RedirectToAction("TransactionHistory");
                }
                catch (VerificationRequiredException ex)
                {
                    var transactionConfig = new TransactionConfirmationViewModel
                    {
                        WalletId = ex.WalletId.GetValueOrDefault(),  // Use GetValueOrDefault for nullable int
                        Amount = ex.Amount.GetValueOrDefault(),
                        Description = ex.Description,
                        TransactionType = TransactionType.Transfer.ToString(),
                        TransactionToken = ex.TransactionToken,
                        RecipinetWalletId = ex.RecipientWallet
                    };


                    return RedirectToAction("ConfirmTransaction", transactionConfig);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View("TransferBetweenWallets", model);
        }
    }

}
