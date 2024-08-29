using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;

namespace Wallet.MVC.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly ITransactionService _transactionService;
        private readonly IContactService _contactService;
        public WalletController(IWalletService walletService, ITransactionService transactionService, IContactService contactService)
        {
            _walletService = walletService;
            _transactionService = transactionService;
            _contactService = contactService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserWalletRequest model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.UserData);
                await _walletService.CreateWallet(model, userId);
                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallet = await _walletService.GetWalletAsync(id, userId);

            if (wallet == null)
            {
                return NotFound();
            }

            var transactions = await _transactionService.FilterTransactionsAsync(
                page: 1,
                pageSize: 50,
                new TransactionRequestFilter { WalletId = id },
                userId
            );
            var model = new WalletDetailsViewModel
            {
                Id = wallet.Id,
                Name = wallet.Name,
                Balance = wallet.Balance,
                Currency = wallet.Currency.ToString(),
                WalletType = wallet.WalletType.ToString(),
                Members = wallet.WalletType == WalletType.Joint
                    ? wallet.AppUserWallets.Select(u => new MemberViewModel
                    {
                        UserName = u.UserName,
                        FirstName = u.FirstName, // Assuming these properties exist
                        LastName = u.LastName   // Assuming these properties exist
                    }).ToList()
                    : null,
                Transactions = transactions.Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    Date = t.Date,
                    Amount = t.Amount,
                    Description = t.Description,
                    Type = t.TransactionType.ToString(),
                    Direction = t.Direction,
                    IsRecurring = t.IsReccuring,
                    RecurrenceInterval = t.RecurrenceInterval
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddUsersToJointWallet(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var contacts = await _contactService.GetContactsAsync(userId);

            var model = new AddUserToJointWalletViewModel
            {
                WalletId = walletId,
                Contacts = contacts.Select(c => new SelectListItem
                {
                    Value = c.Id,
                    Text = $"{c.FirstName} {c.LastName}"
                }).ToList() ?? new List<SelectListItem>(),
                CanSpend = new Dictionary<string, bool>(),
                CanAddFunds = new Dictionary<string, bool>()
            };

            foreach (var contact in model.Contacts)
            {
                model.CanSpend[contact.Value] = false; // Default value
                model.CanAddFunds[contact.Value] = false; // Default value
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddUsersToJointWallet(AddUserToJointWalletViewModel model)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.UserData);

            if (ModelState.IsValid && model.SelectedUserIds != null)
            {
                try
                {
                    foreach (var userId in model.SelectedUserIds)
                    {
                        await _walletService.AddMemberToJointWalletAsync(
                            model.WalletId,
                            userId,
                            model.CanSpend[userId],
                            model.CanAddFunds[userId],
                            ownerId
                        );
                    }

                    return RedirectToAction("Details", new { id = model.WalletId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            // Reload the contacts in case of an error
            var userIdReload = User.FindFirstValue(ClaimTypes.UserData);
            var contacts = await _contactService.GetContactsAsync(userIdReload);
            model.Contacts = contacts.Select(c => new SelectListItem
            {
                Value = c.Id,
                Text = $"{c.FirstName} {c.LastName}"
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> ManageJointWalletMembers(int walletId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);
            var wallet = await _walletService.GetWalletAsync(walletId, userId);

            if (wallet == null || wallet.OwnerId != userId)
            {
                return Unauthorized(); // or a view that displays an error message
            }

            var contacts = await _contactService.GetContactsAsync(userId);
            var members = await _walletService.GetWalletMembersAsync(walletId); // Assuming you have this method

            var model = new ManageJointWalletMembersViewModel
            {
                WalletId = wallet.Id,
                WalletName = wallet.Name,
                Contacts = contacts.ToList(),
                Members = members,
                OwnerId = userId
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddMember(int walletId, string userId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _walletService.AddMemberToJointWalletAsync(walletId, userId, canSpend: true, canAddFunds: true, ownerId: ownerId);
                TempData["SuccessMessage"] = "Member added successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("ManageJointWalletMembers", new { walletId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int walletId, string userId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _walletService.RemoveMemberFromJointWalletAsync(walletId, userId, ownerId);
                TempData["SuccessMessage"] = "Member removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("ManageJointWalletMembers", new { walletId });
        }



    }
}
