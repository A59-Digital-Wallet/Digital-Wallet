using Wallet.Data.Models;

namespace Wallet.Data.Repositories.Contracts
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetUserCategoriesAsync(string userId, int pageNumber, int pageSize);
        Task<Category> GetCategoryByIdAsync(int categoryId);
        Task<bool> AddCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(Category category);
    }
}