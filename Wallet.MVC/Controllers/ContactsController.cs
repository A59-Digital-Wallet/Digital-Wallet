using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallet.Common.Exceptions;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Wallet.MVC.Controllers
{
    [Authorize]
   
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

        [HttpGet]
        
        public async Task<IActionResult> SearchContacts(string searchQuery)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                // Return all contacts if the search query is empty
                var contacts = await _contactService.GetContactsAsync(userId);
                return PartialView("_ContactsListPartial", contacts);
            }

            var searchResults = await _contactService.SearchForContactsAsync(userId, searchQuery);
            return PartialView("_ContactsListPartial", searchResults);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContact(ContactRequest model)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            if (ModelState.IsValid)
            {
                await _contactService.AddContactAsync(userId, model.ContactId);
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
