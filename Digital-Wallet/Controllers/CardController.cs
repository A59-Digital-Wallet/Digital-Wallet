using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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

        /// <summary>
        /// Retrieves a list of cards for the authenticated user.
        /// </summary>
        /// <returns>A list of card details associated with the authenticated user.</returns>
        /// <response code="200">Returns a list of cards for the authenticated user.</response>
        /// <response code="400">If the cards could not be retrieved.</response>
        [Authorize]
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieves a list of cards for the authenticated user.")]
        [SwaggerResponse(200, "Returns a list of cards for the authenticated user.")]
        [SwaggerResponse(400, "If the cards could not be retrieved.")]
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

        /// <summary>
        /// Retrieves a specific card by its ID for the authenticated user.
        /// </summary>
        /// <param name="cardId">The ID of the card to retrieve.</param>
        /// <returns>The card details if found and authorized; otherwise, a 403 or 404 error.</returns>
        /// <response code="200">Returns the card details if the card exists and belongs to the user.</response>
        /// <response code="403">If the user is not authorized to access the card.</response>
        /// <response code="404">If the card is not found.</response>
        [Authorize]
        [HttpGet("{cardId}")]
        [SwaggerOperation(Summary = "Retrieves a specific card by its ID for the authenticated user.")]
        [SwaggerResponse(200, "Returns the card details if the card exists and belongs to the user.")]
        [SwaggerResponse(403, "If the user is not authorized to access the card.")]
        [SwaggerResponse(404, "If the card is not found.")]
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

        /// <summary>
        /// Adds a new card for the authenticated user.
        /// </summary>
        /// <param name="cardRequest">The details of the card to add.</param>
        /// <returns>A message indicating the card was added successfully.</returns>
        /// <response code="200">Returns a success message if the card is added successfully.</response>
        /// <response code="400">If the card data is invalid.</response>
        [Authorize]
        [HttpPost("add")]
        [SwaggerOperation(Summary = "Adds a new card for the authenticated user.")]
        [SwaggerResponse(200, "Returns a success message if the card is added successfully.")]
        [SwaggerResponse(400, "If the card data is invalid.")]
        public async Task<IActionResult> AddCard([FromBody] CardRequest cardRequest)
        {
            var userID = User.FindFirstValue(ClaimTypes.UserData);
            await _cardService.AddCardAsync(cardRequest, userID);
            return Ok(new { message = Messages.Controller.CardAddedSuccessful });
        }

        /// <summary>
        /// Deletes a specific card by its ID for the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the card to delete.</param>
        /// <returns>A message indicating the card was deleted successfully.</returns>
        /// <response code="200">Returns a success message if the card is deleted successfully.</response>
        /// <response code="403">If the user is not authorized to delete the card.</response>
        /// <response code="404">If the card is not found.</response>
        [Authorize]
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Deletes a specific card by its ID for the authenticated user.")]
        [SwaggerResponse(200, "Returns a success message if the card is deleted successfully.")]
        [SwaggerResponse(403, "If the user is not authorized to delete the card.")]
        [SwaggerResponse(404, "If the card is not found.")]
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
