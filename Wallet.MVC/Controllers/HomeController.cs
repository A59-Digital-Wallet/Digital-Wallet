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

        public HomeController(IWalletService walletService, ICardService cardService, ITransactionService transactionService, IContactService contactService)
        {
            _walletService = walletService;
            _cardService = cardService;
            _transactionService = transactionService;
            _contactService = contactService;
        }



        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallets = await _walletService.GetUserWalletsAsync(userId);
            var cards = await _cardService.GetCardsAsync(userId);
            var transactionRequest = new TransactionRequestFilter();
            var transactions = await _transactionService.FilterTransactionsAsync(1, 3, transactionRequest, userId);
            var contacts = await _contactService.GetContactsAsync(userId);
            var recentContacts = contacts.Take(5).ToList();

            var model = new HomeViewModel
            {
                Wallets = wallets.Select(wallet => new WalletViewModel
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Balance = wallet.Balance,
                    Currency = wallet.Currency.ToString()
                }).ToList(),
                Card = cards.FirstOrDefault(),
                Transactions = transactions.Select(transaction => new TransactionViewModel
                {
                    Id = transaction.Id,
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    Type = transaction.TransactionType.ToString(),
                    Direction = transaction.Direction, // Direction now determined by the service
                    IsRecurring = transaction.IsReccuring,
                    RecurrenceInterval = transaction.RecurrenceInterval,
                    
                }).ToList(),
                Contacts = recentContacts
            };

            return View(model);
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
