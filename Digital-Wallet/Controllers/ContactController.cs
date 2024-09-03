using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Digital_Wallet.Controllers
{
    [Route("api/contact")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// Retrieves a list of contacts for the authenticated user.
        /// </summary>
        /// <returns>A list of contact details associated with the authenticated user.</returns>
        /// <response code="200">Returns a list of contacts for the authenticated user.</response>
        /// <response code="400">If the contacts could not be retrieved.</response>
        [Authorize]
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieves a list of contacts for the authenticated user.")]
        [SwaggerResponse(200, "Returns a list of contacts for the authenticated user.", typeof(ICollection<ContactResponseDTO>))]
        [SwaggerResponse(400, "If the contacts could not be retrieved.")]
        public async Task<IActionResult> GetContactsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                ICollection<ContactResponseDTO> response = await _contactService.GetContactsAsync(userId);
                return Ok(response);
            }
            catch (EntityNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a new contact for the authenticated user.
        /// </summary>
        /// <param name="contactId">The ID of the contact to add.</param>
        /// <returns>A success message if the contact is added successfully.</returns>
        /// <response code="200">If the contact is added successfully.</response>
        /// <response code="404">If the contact is not found.</response>
        /// <response code="400">If the operation is invalid.</response>
        [Authorize]
        [HttpPost("addContact")]
        [SwaggerOperation(Summary = "Adds a new contact for the authenticated user.")]
        [SwaggerResponse(200, "If the contact is added successfully.")]
        [SwaggerResponse(404, "If the contact is not found.")]
        [SwaggerResponse(400, "If the operation is invalid.")]
        public async Task<IActionResult> AddContact(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _contactService.AddContactAsync(userId, contactId);
                return Ok(new { message = Messages.Controller.ContactAddedSuccessful });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes a specific contact for the authenticated user.
        /// </summary>
        /// <param name="contactId">The ID of the contact to remove.</param>
        /// <returns>A success message if the contact is removed successfully.</returns>
        /// <response code="200">If the contact is removed successfully.</response>
        /// <response code="404">If the contact is not found.</response>
        [Authorize]
        [HttpDelete("delete")]
        [SwaggerOperation(Summary = "Removes a specific contact for the authenticated user.")]
        [SwaggerResponse(200, "If the contact is removed successfully.")]
        [SwaggerResponse(404, "If the contact is not found.")]
        public async Task<IActionResult> RemoveContatctAsync(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _contactService.RemoveContactAsync(userId, contactId);
                return Ok(new { message = Messages.Controller.ContactDeletedSuccessful });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
