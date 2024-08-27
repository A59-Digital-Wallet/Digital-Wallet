using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;

namespace Digital_Wallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly ICardService _cardService;
        private readonly UserManager<AppUser> _userManager;
        public CardController(ICardService cardService, UserManager<AppUser> userManager)
        {
            _cardService = cardService;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCards()
        {
            var userID = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                var cards = await _cardService.GetCardsAsync(userID);
                return Ok(cards);
            }
            catch (EntityNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{cardId}")]
        public async Task<IActionResult> GetCard(int cardId)
        {
            var userID = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                var card = await _cardService.GetCardAsync(cardId, userID);
                return Ok(card);
            }
            catch (AuthorizationException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddCard([FromBody] CardRequest cardRequest)
        {
            var userID = User.FindFirstValue(ClaimTypes.UserData);
            await _cardService.AddCardAsync(cardRequest, userID);
            return Ok(new { message = Messages.Controller.CardAddedSuccessful });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            var userID = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _cardService.DeleteCardAsync(id, userID);
            }
            catch (AuthorizationException ex)
            {
                return Forbid(ex.Message);
            }
            return Ok(new { message = Messages.Controller.CardDeletedSuccessful });
        }
    }
}
