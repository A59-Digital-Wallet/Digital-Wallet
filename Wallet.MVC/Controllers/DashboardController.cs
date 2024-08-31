using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Response;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{
    
    public class DashboardController : Controller
    {

        private readonly IWalletService _walletService;
        private readonly ICardService _cardService;
        private readonly ITransactionService _transactionService;
        private readonly IContactService _contactService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICategoryService _categoryService;
        private readonly IMoneyRequestService _moneyRequestService;
        private readonly IStatsService _statsService;
        public DashboardController(IWalletService walletService, ICardService cardService, ITransactionService transactionService, 
            IContactService contactService, UserManager<AppUser> userManager, 
            ICategoryService categoryService, IMoneyRequestService moneyRequestService,
            IStatsService statsService)
        {
            _walletService = walletService;
            _cardService = cardService;
            _transactionService = transactionService;
            _contactService = contactService;
            _userManager = userManager;
            _categoryService = categoryService;
            _moneyRequestService = moneyRequestService;
            _statsService = statsService;
        }


        public async Task<IActionResult> Index(int? walletId, string interval = "daily")
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData); // Or ClaimTypes.UserData based on your setup

            var user = await _userManager.FindByIdAsync(userId);

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var wallets = await _walletService.GetUserWalletsAsync(userId);

            // If there are no wallets, redirect to wallet creation
            if (!wallets.Any())
            {
                return RedirectToAction("Create", "Wallet");
            }
            
            // If a walletId is passed in, update the user's LastSelectedWalletId
            if (walletId.HasValue)
            {
                
                    user.LastSelectedWalletId = walletId.Value;
                    await _userManager.UpdateAsync(user); // Save the user's preference to the database
                
            }

            // Retrieve the last selected wallet ID from the user record
            var preferredWalletId = user.LastSelectedWalletId;

            // Select the wallet based on the preferredWalletId or default to the first wallet
            var selectedWallet = wallets.FirstOrDefault(w => w.Id == preferredWalletId)
                                 ?? wallets.FirstOrDefault();
            
            user.LastSelectedWalletId = selectedWallet.Id;
            await _userManager.UpdateAsync(user);


            // Set the selected wallet in ViewBag for use in the view
            var culture = CurrencyHelper.GetCurrencyCulture(selectedWallet.Currency.ToString());
            ViewBag.SelectedWallet = new WalletViewModel
            {
                Id = selectedWallet.Id,
                Name = selectedWallet.Name,
                Balance = selectedWallet.Balance,
                Currency = selectedWallet.Currency.ToString(),
                CurrencyCulture = culture,
                Type = selectedWallet.WalletType.ToString(),
                OwnerId = selectedWallet.OwnerId
            };

            var isOwnerOfJointWallet = selectedWallet.WalletType == WalletType.Joint && selectedWallet.OwnerId == userId;
            ViewBag.ShowManageMembersButton = isOwnerOfJointWallet;


            // Filter transactions based on the selected wallet
            var transactionRequest = new TransactionRequestFilter
            {
                WalletId = selectedWallet.Id,
                OrderBy = "desc"
            };
            var transactions = await _transactionService.FilterTransactionsAsync(1, 5, transactionRequest, userId);

            // Get the user's contacts
            var contacts = await _contactService.GetContactsAsync(userId);
            var recentContacts = contacts.Take(1).ToList();
            List<CategoryViewModel> categories = new List<CategoryViewModel>();
            try
            {
                var categoryList = await _categoryService.GetUserCategoriesAsync(userId, pageNumber: 1, pageSize: 10);
                categories = categoryList.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Transactions = c.Transactions?.Select(t => new TransactionViewModel
                    {
                        Id = t.Id,
                        Date = t.Date,
                        Amount = t.Amount,
                        Description = t.Description,
                        Type = t.TransactionType.ToString(),
                        FromWallet = t.WalletName,
                        Direction = t.Status.ToString(),
                        RecurrenceInterval = t.RecurrenceInterval
                    }).ToList()
                }).Take(3).ToList();
            }
            catch (EntityNotFoundException ex)
            {
                ViewBag.ErrorMessage = ex.Message; // Pass the error message to the view
            }
            var cards = await _cardService.GetCardsAsync(userId);
            var receivedRequests = await _moneyRequestService.GetReceivedRequestsAsync(userId);
            var (weeklyLabels, weeklyAmounts) = await _transactionService.GetWeeklySpendingAsync(selectedWallet.Id);
            var spendingByCategory = await _transactionService.GetMonthlySpendingByCategoryAsync(userId, selectedWallet.Id);
          
            var dailyBalanceData = await _statsService.GetBalanceOverTime(selectedWallet.Id, interval, userId);

            // Build the HomeViewModel with all the necessary data
            var model = new HomeViewModel
            {
                Wallets = wallets.Select(wallet => new WalletViewModel
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Balance = wallet.Balance,
                    Currency = wallet.Currency.ToString(),
                    Type = wallet.WalletType.ToString(),
                    CurrencyCulture = CurrencyHelper.GetCurrencyCulture(wallet.Currency.ToString())
                }).ToList(),

                Card = cards.FirstOrDefault(),
                MonthlySpendingByCategory = spendingByCategory,
                Transactions = transactions.Select(transaction => new TransactionViewModel
                {
                    Id = transaction.Id,
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    Type = transaction.TransactionType.ToString(),
                    Direction = transaction.Direction,
                    IsRecurring = transaction.IsReccuring,
                    OriginalCurrency = transaction.OriginalCurrency,
                    OriginalAmount = transaction.OriginalAmount,
                    RecurrenceInterval = transaction.RecurrenceInterval,
                    CurrencyCulture = CurrencyHelper.GetCurrencyCulture(transaction.OriginalCurrency),
                    CurrencyCultureSent = CurrencyHelper.GetCurrencyCulture(transaction.SentCurrency),
                }).ToList(),

                Contacts = recentContacts.Select(contact => new ContactResponseDTO
                {
                    Id = contact.Id,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    ProfilePictureURL = contact.ProfilePictureURL
                }).ToList(),
                WeeklySpendingLabels = weeklyLabels,  // Pass the weekly labels to the view model
                WeeklySpendingAmounts = weeklyAmounts,  // Pass the weekly spending amounts to the view model
                TotalSpentThisMonth = weeklyAmounts.Sum(),
                Categories = categories,
                ReceivedRequests = receivedRequests.ToList(),
                DailyBalanceLabels = dailyBalanceData.Item1,
                DailyBalanceAmounts = dailyBalanceData.Item2,
                SelectedInterval = interval,
                profilePicture = user.ProfilePictureURL
                // Example of other potential properties

            };
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    balanceLabels = model.DailyBalanceLabels,
                    balanceAmounts = model.DailyBalanceAmounts
                });
            }

            return View(model);
        }

        
        // This method handles setting the preferred wallet
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult SetPreferredWallet(int walletId)
        {
            // Update the preferred wallet in the session
            HttpContext.Session.SetInt32("PreferredWalletId", walletId);

            // Redirect back to the Index action to reload the home page with the new wallet
            return RedirectToAction("Index");
        }



        public IActionResult Privacy()
        {
            return View();
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }
}
