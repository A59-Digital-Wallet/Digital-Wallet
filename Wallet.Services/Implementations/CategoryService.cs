using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryFactory _categoryFactory;

        public CategoryService(ICategoryRepository categoryRepository, ICategoryFactory categoryFactory)
        {
            _categoryRepository = categoryRepository;
            _categoryFactory = categoryFactory;
        }

        public async Task<List<CategoryResponseDTO>> GetUserCategoriesAsync(string userId, int pageNumber, int pageSize)
        {
            List<Category> categories = await _categoryRepository.GetUserCategoriesAsync(userId, pageNumber, pageSize);

            if (categories == null || !categories.Any())
            {
                throw new EntityNotFoundException("No categories found!");
            }

            List<CategoryResponseDTO> response = _categoryFactory.Map(categories);
            return response;
        }


        public async Task AddCategoryAsync(string userId, CategoryRequestDTO categoryRequest)
        {
            if (categoryRequest == null)
            {
                throw new ArgumentNullException("Name of category cannot be empty!");
            }

            Category category = _categoryFactory.Map(userId, categoryRequest);

            bool isAdded = await _categoryRepository.AddCategoryAsync(category);

            if (!isAdded)
            {
                throw new InvalidOperationException("Failed to save the category. Please try again.");
            }
        }

        public async Task<CategoryResponseDTO> UpdateCategoryAsync(string userId, int categoryId, CategoryRequestDTO categoryRequest)
        {
            if (categoryRequest == null)
            {
                throw new ArgumentNullException("Name of category cannot be empty!");
            }

            Category categoryExists = await _categoryRepository.GetCategoryById(categoryId);

            if (categoryExists == null)
            {
                throw new EntityNotFoundException($"Category does not exist.");
            }
            else if (categoryExists.UserId != userId)
            {
                throw new AuthorizationException("You are not authorized to edit this category");
            }

            // Update the existing entity's properties
            categoryExists.Name = categoryRequest.Name;
            // Update other properties as needed

            Category updatedCategory = await _categoryRepository.UpdateCategoryAsync(categoryExists);
            CategoryResponseDTO response = _categoryFactory.Map(updatedCategory);
            return response;
        }

        public async Task DeleteCategoryAsync(string userId, int categoryId)
        {
            Category category = await _categoryRepository.GetCategoryById(categoryId);

            if (category == null)
            {
                throw new EntityNotFoundException("Category not found.");
            }

            if (category.UserId != userId)
            {
                throw new AuthorizationException("You are not authorized to delete this category.");
            }

            bool result = await _categoryRepository.DeleteCategoryAsync(category);

            if (!result)
            {
                throw new Exception("An error occurred while deleting the category.");
            }
        }
    }
}
