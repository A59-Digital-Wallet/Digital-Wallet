using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallet.DTO.Request;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{

    [Authorize]
    public class CardController : Controller
    {
        private readonly ICardService _cardService;

        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;

            return View();
        }
        public async Task<IActionResult> ShowAll()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            var cards = await _cardService.GetCardsAsync(userId);
            return View(cards);
        }

        [HttpGet]
        public IActionResult AddCard()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCard(CardRequest cardRequest)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
                await _cardService.AddCardAsync(cardRequest, userId);
                return View();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCard(int cardId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            await _cardService.DeleteCardAsync(cardId, userId);
            return RedirectToAction("Index", "Home");
        }
    }

}
