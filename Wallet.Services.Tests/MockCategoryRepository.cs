using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Services.Tests.Mocks
{
    public class MockCategoryRepository
    {
        private readonly List<Category> _categories;
        private readonly Mock<ICategoryRepository> _mockRepository;

        public MockCategoryRepository()
        {
            _categories = new List<Category>
            {
                new Category { Id = 1, Name = "Food", UserId = "user1", Transactions = new List<Transaction>() },
                new Category { Id = 2, Name = "Transport", UserId = "user1", Transactions = new List<Transaction>() },
                new Category { Id = 3, Name = "Utilities", UserId = "user2", Transactions = new List<Transaction>() }
            };

            _mockRepository = new Mock<ICategoryRepository>();

            SetupMocks();
        }

        private void SetupMocks()
        {
            // Mock GetCategoryByIdAsync
            _mockRepository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int categoryId) => _categories.FirstOrDefault(c => c.Id == categoryId));

            // Mock GetUserCategoriesAsync
            _mockRepository.Setup(repo => repo.GetUserCategoriesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((string userId, int pageNumber, int pageSize) =>
                    _categories.Where(c => c.UserId == userId)
                               .Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToList());

            // Mock AddCategoryAsync
            _mockRepository.Setup(repo => repo.AddCategoryAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category category) =>
                {
                    if (_categories.Any(c => c.Name == category.Name && c.UserId == category.UserId))
                    {
                        return false; // Simulate duplicate category scenario
                    }

                    category.Id = _categories.Max(c => c.Id) + 1; // Simulate auto-increment ID
                    _categories.Add(category);
                    return true;
                });

            // Mock UpdateCategoryAsync
            _mockRepository.Setup(repo => repo.UpdateCategoryAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category category) =>
                {
                    var existingCategory = _categories.FirstOrDefault(c => c.Id == category.Id);
                    if (existingCategory == null) return null;

                    existingCategory.Name = category.Name;
                    return existingCategory;
                });

            // Mock DeleteCategoryAsync
            _mockRepository.Setup(repo => repo.DeleteCategoryAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category category) =>
                {
                    var existingCategory = _categories.FirstOrDefault(c => c.Id == category.Id);
                    if (existingCategory != null)
                    {
                        _categories.Remove(existingCategory);
                        return true;
                    }
                    return false;
                });
        }

        public Mock<ICategoryRepository> Mock()
        {
            return _mockRepository;
        }
    }
}
