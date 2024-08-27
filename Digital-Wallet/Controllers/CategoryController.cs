using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Digital_Wallet.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserCategories(int pageNumber = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                List<CategoryResponseDTO> categories = await _categoryService.GetUserCategoriesAsync(userId, pageNumber, pageSize);
                return Ok(categories);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryRequestDTO categoryRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _categoryService.AddCategoryAsync(userId, categoryRequest);
                return Ok(new { message = Messages.Controller.CategoryAddedSuccessful });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryRequestDTO categoryRequest)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                CategoryResponseDTO updatedCategory = await _categoryService.UpdateCategoryAsync(userId, categoryId, categoryRequest);
                return Ok(updatedCategory);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (AuthorizationException ex)
            {
                return Forbid(ex.Message);
            }

        }

        [Authorize]
        [HttpDelete("delete/{categoryId}")]
        public async Task<IActionResult> DeleteCategoryAsync(int categoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.UserData);

            try
            {
                await _categoryService.DeleteCategoryAsync(userId, categoryId);
                return Ok(new { message = Messages.Controller.CategoryDeletedSuccessful });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (AuthorizationException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
