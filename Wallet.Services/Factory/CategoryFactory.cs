using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Factory
{
    public class CategoryFactory : ICategoryFactory
    {
        public List<CategoryResponseDTO> Map(List<Category> categories)
        {
            List<CategoryResponseDTO> response = categories.Select(category => new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Transactions = category.Transactions?.Select(transaction => new TransactionDto
                {
                    Id = transaction.Id,
                    Amount = transaction.OriginalAmount,
                    Date = transaction.Date,
                    Description = transaction.Description,
                    Status = transaction.Status,
                    WalletName = transaction.Wallet?.Name ?? "Unknown Wallet",
                    TransactionType = transaction.TransactionType
                }).ToList() ?? new List<TransactionDto>()
            }).ToList();

            return response;
        }

        public Category Map(string userId, CategoryRequestDTO categoryRequest)
        {
            Category category = new Category()
            {
                UserId = userId,
                Name = categoryRequest.Name,

            };
            return category;
        }

        public Category Map(string userId, int categoryId, CategoryRequestDTO categoryRequest)
        {
            Category category = new Category()
            {
                Id = categoryId,
                UserId = userId,
                Name = categoryRequest.Name,
            };
            return category;
        }

        public CategoryResponseDTO Map(Category category)
        {
            CategoryResponseDTO response = new CategoryResponseDTO()
            {
                Name = category.Name,
            };
            return response;
        }
    }
}
