using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Common.Exceptions;
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
        [HttpPost("add")]
        public async Task<IActionResult> AddCard([FromBody] CardRequest cardRequest)
        {
            var userID = User.FindFirstValue(ClaimTypes.UserData);
            await _cardService.AddCardAsync(cardRequest, userID);
            return Ok();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCard(int id)
        {
            var userID = User.FindFirstValue(ClaimTypes.UserData);
            var card = await _cardService.GetCardAsync(id);
            if (userID != card.AppUserId)
            {
                return BadRequest();
            }
            return Ok();
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
            catch(AuthorizationException ex)
            {
                return Forbid(ex.Message);
            }
            return Ok( new { message = "Card deleted successfully."});    
        }
    }
}
