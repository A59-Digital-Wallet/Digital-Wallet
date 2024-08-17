using Microsoft.AspNetCore.Mvc;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using System.Threading.Tasks;
using System.Security.Claims;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models;
using Wallet.MVC.Models;
using Wallet.DTO.Response;

namespace Wallet.MVC.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly ITransactionService _transactionService;
        public WalletController(IWalletService walletService, ITransactionService transactionService)
        {
            _walletService = walletService;
            _transactionService = transactionService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserWalletRequest model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                await _walletService.CreateWallet(model, userId);
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallet = await _walletService.GetWalletAsync(id, userId);

            if (wallet == null)
            {
                return NotFound();
            }

            var transactions = await _transactionService.FilterTransactionsAsync(
                page: 1,
                pageSize: 50,
                new TransactionRequestFilter { WalletId = id },
                userId
            );

            var model = new WalletDetailsViewModel
            {
                Id = wallet.Id,
                Name = wallet.Name,
                Balance = wallet.Balance,
                Currency = wallet.Currency.ToString(),
                WalletType = wallet.WalletType.ToString(),
                Members = wallet.WalletType == WalletType.Joint ? wallet.AppUserWallets.Select(u => u.UserName).ToList() : null,
                Transactions = transactions.Select(t => new TransactionViewModel
                {
                    Date = t.Date,
                    Amount = t.Amount,
                    Description = t.Description,
                    Type = t.TransactionType.ToString(),
                    Direction = DetermineDirection(t, wallet.Id) // Determine direction
                }).ToList()
            };

            return View(model);
        }

        private string DetermineDirection(TransactionDto transaction, int walletId)
        {
            // Incoming if it's a deposit or a transfer to this wallet
            if (transaction.TransactionType == TransactionType.Deposit ||
                (transaction.TransactionType == TransactionType.Transfer && transaction.RecepientWalledId == walletId))
            {
                return "Incoming";
            }

            // Outgoing if it's a withdrawal or a transfer from this wallet
            return "Outgoing";
        }
    }
}
