using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
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

        [Authorize]
        [HttpGet()]
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
        [Authorize]
        [HttpPost("addContact")]
        public async Task<IActionResult> AddContact(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _contactService.AddContactAsync(userId, contactId);
                return Ok(new { message = "Contact Added."});
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

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> RemoveContatctAsync(string contactId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _contactService.RemoveContactAsync(userId, contactId);
                return Ok(new { message = "Contact removed." });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
