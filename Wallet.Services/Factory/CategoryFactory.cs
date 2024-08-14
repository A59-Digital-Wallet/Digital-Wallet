using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Response;
using Wallet.DTO.Request;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Factory
{
    public class CategoryFactory : ICategoryFactory
    {
        public List<CategoryResponseDTO> Map(List<Category> categories)
        {
            List<CategoryResponseDTO> response = categories.Select(category => new CategoryResponseDTO
            {
                Name = category.Name,
                Transactions = category.Transactions?.Select(transaction => new TransactionDto
                {
                    Amount = transaction.Amount,
                    Date = transaction.Date,
                    Description = transaction.Description,
                    Status = transaction.Status,
                    WalletName = transaction.Wallet.Name,
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
                Name= category.Name,
            };
            return response;
        }
    }
}
