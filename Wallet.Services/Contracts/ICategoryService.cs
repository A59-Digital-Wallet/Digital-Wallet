using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(string userId, CategoryRequestDTO categoryRequest);
        Task DeleteCategoryAsync(string userId, int categoryId);
        Task<List<CategoryResponseDTO>> GetUserCategoriesAsync(string userId, int pageNumber, int pageSize);
        Task<CategoryResponseDTO> UpdateCategoryAsync(string userId, int categoryId, CategoryRequestDTO categoryRequest);
    }
}