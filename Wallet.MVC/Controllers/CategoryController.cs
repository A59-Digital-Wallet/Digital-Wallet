using Microsoft.AspNetCore.Mvc;
using Wallet.MVC.Models;
using Wallet.Services.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Wallet.DTO.Request;

namespace Wallet.MVC.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            var categories = await _categoryService.GetUserCategoriesAsync(userId, pageNumber: 1, pageSize: 10);

            var model = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Transactions = c.Transactions?.Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    Date = t.Date,
                    Amount = t.Amount,
                    Description = t.Description,
                    Type = t.TransactionType.ToString(),
                    FromWallet = t.WalletName,
                    Direction = t.Status.ToString(),
                    RecurrenceInterval = t.RecurrenceInterval
                }).ToList()
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CategoryTransactions(int categoryId)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.UserData)?.Value;
            var categories = await _categoryService.GetUserCategoriesAsync(userId, pageNumber: 1, pageSize: 10);

            var category = categories.FirstOrDefault(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            // Display transactions related to the selected category
            var transactions = category.Transactions.Select(t => new TransactionViewModel
            {
                Id = t.Id,
                Date = t.Date,
                Amount = t.Amount,
                Description = t.Description,
                Type = t.TransactionType.ToString(),
                FromWallet = t.WalletName,
                Direction = t.Status.ToString(),
                RecurrenceInterval = t.RecurrenceInterval
            }).ToList();

            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.UserData);
            var category = await _categoryService.GetCategoryAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryRequestDTO
            {
                Name = category.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, CategoryRequestDTO model)
        {
            string userId = User.FindFirstValue(ClaimTypes.UserData);
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateCategoryAsync(userId, id, model);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            string userId = User.FindFirstValue(ClaimTypes.UserData);
            try
            {
                await _categoryService.DeleteCategoryAsync(userId, categoryId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }


    }
}
