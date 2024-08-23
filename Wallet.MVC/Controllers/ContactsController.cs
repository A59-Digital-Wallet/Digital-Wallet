using Microsoft.AspNetCore.Mvc;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.MVC.Controllers
{
    public class ContactsController : Controller
    {

        private readonly IContactService _contactService;
        private readonly IUserService _userService;

        public ContactsController(IContactService contactService, IUserService userService)
        {
            _contactService = contactService;
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            var contacts = await _contactService.GetContactsAsync(userId);
            return View(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> RecentContacts()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            var contacts = await _contactService.GetContactsAsync(userId);
            var recentContacts = contacts.Take(5).ToList();
            return PartialView("_RecentContacts", recentContacts);
        }

        [HttpGet]
        public async Task<IActionResult> SearchContact(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return PartialView("_SearchResults", new List<UserResponseDTO>());
            }

            var users = await _userService.SearchUsersAsync(searchTerm, 1, 10); // Adjust page and pageSize as needed
            var userDtos = users.Items.Select(user => new UserResponseDTO
            {
                Id = user.User.Id,
                UserName = user.User.UserName,
                Email = user.User.Email
            });

            return PartialView("_SearchResults", userDtos);
        }


        [HttpPost]
        public async Task<IActionResult> AddContact(ContactRequest model)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            if (ModelState.IsValid)
            {
                await _contactService.AddContactAsync(userId, model.ContactId);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveContact(string contactId)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            try
            {
                await _contactService.RemoveContactAsync(userId, contactId);
                return RedirectToAction("Index");
            }
            catch (EntityNotFoundException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View();
        }

        [HttpGet]
        public IActionResult AddContact()
        {
            return View();
        }
    }
}
