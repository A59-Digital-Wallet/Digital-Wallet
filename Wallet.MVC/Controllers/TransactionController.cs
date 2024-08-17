using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
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

                await _transactionService.CreateTransactionAsync(transactionRequest, userId);
                return RedirectToAction("Index", "Home");
            }

            return View("SelectWalletAndCard", model);
        }
    }

}
