using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Factory.Contracts
{
    public interface ICategoryFactory
    {
        List<CategoryResponseDTO> Map(List<Category> categories);
        Category Map(string userId, CategoryRequestDTO cardRequest);
        Category Map(string userId, int categoryId, CategoryRequestDTO categoryRequest);
        CategoryResponseDTO Map(Category category);
    }
}