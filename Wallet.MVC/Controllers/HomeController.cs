using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Data.Migrations;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{
    public class HomeController : Controller
    {

        private readonly IWalletService _walletService;
        private readonly ICardService _cardService;
        private readonly ITransactionService _transactionService;
        private readonly IContactService _contactService;
        private readonly UserManager<AppUser> _userManager;
        public HomeController(IWalletService walletService, ICardService cardService, ITransactionService transactionService, IContactService contactService, UserManager<AppUser> userManager)
        {
            _walletService = walletService;
            _cardService = cardService;
            _transactionService = transactionService;
            _contactService = contactService;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index(int? walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData); // Or ClaimTypes.UserData based on your setup
            if(userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var wallets = await _walletService.GetUserWalletsAsync(userId);
            var cards = await _cardService.GetCardsAsync(userId);

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
                CurrencyCulture = culture // Pass the culture to the ViewBag
            };


            // Filter transactions based on the selected wallet
            var transactionRequest = new TransactionRequestFilter
            {
                WalletId = selectedWallet.Id
            };
            var transactions = await _transactionService.FilterTransactionsAsync(1, 3, transactionRequest, userId);

            // Get the user's contacts
            var contacts = await _contactService.GetContactsAsync(userId);
            var recentContacts = contacts.Take(5).ToList();
            var (weeklyLabels, weeklyAmounts) = await _transactionService.GetWeeklySpendingAsync(selectedWallet.Id);
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
                    CurrencyCulture = CurrencyHelper.GetCurrencyCulture(transaction.OriginalCurrency)
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

                // Example of other potential properties

            };

            return View(model);
        }



        // This method handles setting the preferred wallet
        [HttpPost]
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
