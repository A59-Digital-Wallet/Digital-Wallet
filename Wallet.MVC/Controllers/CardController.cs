using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers; 

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
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = Messages.Controller.PageOrPageSizeInvalid; 
                return View(cardRequest); 
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
                await _cardService.AddCardAsync(cardRequest, userId);
                TempData["SuccessMessage"] = Messages.Controller.CardAddedSuccessful; 
                return RedirectToAction("ShowAll"); 
            }
            catch (ArgumentException ex)
            {
                // Split the validation error messages
                var errors = ex.Message.Split("; ");
                foreach (var error in errors)
                {
                    if (error.Contains("Invalid card number"))
                    {
                        ModelState.AddModelError("CardNumber", Messages.Controller.InvalidCardNumber);
                    }
                    else if (error.Contains("Invalid card number length"))
                    {
                        ModelState.AddModelError("CardNumber", Messages.Controller.InvalidCardNumberLength);
                    }
                    else if (error.Contains("nearing its expiration"))
                    {
                        ModelState.AddModelError("ExpiryDate", Messages.Controller.CardNearingExpiration);
                    }
                    else if (error.Contains("Invalid CVV"))
                    {
                        ModelState.AddModelError("CVV", Messages.Controller.InvalidCVV);
                    }
                    else if (error.Contains("Invalid cardholder name"))
                    {
                        ModelState.AddModelError("CardHolderName", Messages.Controller.InvalidCardholderName);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message); 
            }
            catch (Exception)
            {
                ModelState.AddModelError("", Messages.OperationFailed);
            }

            return View(cardRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCard(int cardId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
                await _cardService.DeleteCardAsync(cardId, userId);
                TempData["SuccessMessage"] = Messages.Controller.CardDeletedSuccessful; 
            }
            catch (EntityNotFoundException ex)
            {
                TempData["ErrorMessage"] = Messages.Service.CardNotFound; 
            }
            catch (AuthorizationException ex)
            {
                TempData["ErrorMessage"] = Messages.Unauthorized; 
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = Messages.OperationFailed; 
            }

            return RedirectToAction("ShowAll");
        }
    }
}
