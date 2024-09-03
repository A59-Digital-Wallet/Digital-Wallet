using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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

        /// <summary>
        /// Retrieves a paginated list of categories for the authenticated user.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of categories per page (default is 10).</param>
        /// <returns>A list of categories for the authenticated user.</returns>
        /// <response code="200">Returns a list of categories for the user.</response>
        /// <response code="404">If no categories are found for the user.</response>
        [Authorize]
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieves a paginated list of categories for the authenticated user.")]
        [SwaggerResponse(200, "Returns a list of categories for the user.", typeof(List<CategoryResponseDTO>))]
        [SwaggerResponse(404, "If no categories are found for the user.")]
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

        /// <summary>
        /// Adds a new category for the authenticated user.
        /// </summary>
        /// <param name="categoryRequest">The details of the category to add.</param>
        /// <returns>A success message if the category is added successfully.</returns>
        /// <response code="200">If the category is added successfully.</response>
        /// <response code="400">If the category data is invalid.</response>
        [Authorize]
        [HttpPost("add")]
        [SwaggerOperation(Summary = "Adds a new category for the authenticated user.")]
        [SwaggerResponse(200, "If the category is added successfully.")]
        [SwaggerResponse(400, "If the category data is invalid.")]
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

        /// <summary>
        /// Updates an existing category for the authenticated user.
        /// </summary>
        /// <param name="categoryId">The ID of the category to update.</param>
        /// <param name="categoryRequest">The new details of the category.</param>
        /// <returns>The updated category details.</returns>
        /// <response code="200">Returns the updated category details.</response>
        /// <response code="400">If the category data is invalid.</response>
        /// <response code="404">If the category is not found.</response>
        /// <response code="403">If the user is not authorized to update the category.</response>
        [Authorize]
        [HttpPut("update/{categoryId}")]
        [SwaggerOperation(Summary = "Updates an existing category for the authenticated user.")]
        [SwaggerResponse(200, "Returns the updated category details.", typeof(CategoryResponseDTO))]
        [SwaggerResponse(400, "If the category data is invalid.")]
        [SwaggerResponse(404, "If the category is not found.")]
        [SwaggerResponse(403, "If the user is not authorized to update the category.")]
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

        /// <summary>
        /// Deletes a specific category by its ID for the authenticated user.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <returns>A success message if the category is deleted successfully.</returns>
        /// <response code="200">If the category is deleted successfully.</response>
        /// <response code="404">If the category is not found.</response>
        /// <response code="403">If the user is not authorized to delete the category.</response>
        [Authorize]
        [HttpDelete("delete/{categoryId}")]
        [SwaggerOperation(Summary = "Deletes a specific category by its ID for the authenticated user.")]
        [SwaggerResponse(200, "If the category is deleted successfully.")]
        [SwaggerResponse(404, "If the category is not found.")]
        [SwaggerResponse(403, "If the user is not authorized to delete the category.")]
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
