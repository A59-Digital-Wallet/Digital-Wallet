using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;

namespace Digital_Wallet.Controllers
{
    [Route("api/card")]
    [ApiController]
    public class CardController : ControllerBase
    {
        ICardService _cardService;
        public CardController(ICardService cardService)
        {
            _cardService = cardService;
        }
        [HttpPost]
        public async Task<IActionResult> AddCard([FromBody] CardRequest cardRequest)
        {
            await _cardService.AddCardAsync(cardRequest);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCard(int id)
        {
            var card = await _cardService.GetCardAsync(id);
            if (card != null)
            {
                return Ok(card);
            }
            return NotFound();
        }
    }
}
