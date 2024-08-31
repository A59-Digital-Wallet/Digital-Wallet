using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Implementations;
using Wallet.Services.Tests.Mocks;
using Wallet.Services.Validation.TransactionValidation;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class CategoryServiceTests
    {
        private MockCategoryRepository _mockCategoryRepository;
        private MockTransactionRepository _mockTransactionRepository;
        private MockTransactionValidator _mockTransactionValidator;
        private Mock<ICategoryFactory> _mockCategoryFactory;
        private CategoryService _categoryService;

        [TestInitialize]
        public void Setup()
        {
            _mockCategoryRepository = new MockCategoryRepository();
            _mockTransactionRepository = new MockTransactionRepository();
            _mockTransactionValidator = new MockTransactionValidator();
            _mockCategoryFactory = new Mock<ICategoryFactory>();

            _categoryService = new CategoryService(
                _mockCategoryRepository.Mock().Object,
                _mockCategoryFactory.Object
            );
        }

        [TestMethod]
        public async Task GetCategoryAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;

            // Act
            var result = await _categoryService.GetCategoryAsync(categoryId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(categoryId, result.Id);
        }

        [TestMethod]
        public async Task GetCategoryAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999; // Non-existent category

            // Act
            var result = await _categoryService.GetCategoryAsync(categoryId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetUserCategoriesAsync_ShouldReturnCategories_WhenCategoriesExist()
        {
            // Arrange
            var userId = "user1";
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Food", UserId = "user1", Transactions = new List<Transaction>() },
                new Category { Id = 2, Name = "Transport", UserId = "user1", Transactions = new List<Transaction>() }
            };

            _mockCategoryFactory.Setup(factory => factory.Map(It.IsAny<List<Category>>()))
                .Returns(categories.Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Transactions = new List<TransactionDto>()
                }).ToList());

            // Act
            var result = await _categoryService.GetUserCategoriesAsync(userId, 1, 10);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count); // user1 has 2 categories in the mock setup
        }

        [TestMethod]
        public async Task AddCategoryAsync_ShouldThrowArgumentNullException_WhenCategoryRequestIsNull()
        {
            // Arrange
            var userId = "user1";

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _categoryService.AddCategoryAsync(userId, null));
        }

        [TestMethod]
        public async Task AddCategoryAsync_ShouldThrowInvalidOperationException_WhenAddCategoryFails()
        {
            // Arrange
            var userId = "user1";
            var categoryRequest = new CategoryRequestDTO { Name = "Food" }; // Duplicate name
            var category = new Category { Id = 1, Name = "Food", UserId = userId };

            _mockCategoryFactory.Setup(factory => factory.Map(userId, categoryRequest)).Returns(category);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _categoryService.AddCategoryAsync(userId, categoryRequest));
        }

        [TestMethod]
        public async Task AddCategoryAsync_ShouldAddCategorySuccessfully()
        {
            // Arrange
            var userId = "user1";
            var categoryRequest = new CategoryRequestDTO { Name = "New Category" };
            var category = new Category { Id = 4, Name = "New Category", UserId = userId };

            _mockCategoryFactory.Setup(factory => factory.Map(userId, categoryRequest)).Returns(category);

            // Act
            await _categoryService.AddCategoryAsync(userId, categoryRequest);

            // Assert
            var addedCategory = await _categoryService.GetCategoryAsync(4);
            Assert.IsNotNull(addedCategory);
            Assert.AreEqual("New Category", addedCategory.Name);
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_ShouldThrowEntityNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var categoryId = 999; // Non-existent category
            var categoryRequest = new CategoryRequestDTO { Name = "Updated Category" };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _categoryService.UpdateCategoryAsync(userId, categoryId, categoryRequest));
        }

        [TestMethod]
        public async Task UpdateCategoryAsync_ShouldThrowAuthorizationException_WhenUserIsNotAuthorized()
        {
            // Arrange
            var userId = "user1";
            var categoryId = 3; // Category belongs to user2
            var categoryRequest = new CategoryRequestDTO { Name = "Updated Category" };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<AuthorizationException>(() => _categoryService.UpdateCategoryAsync(userId, categoryId, categoryRequest));
        }

        [TestMethod]
        public async Task DeleteCategoryAsync_ShouldThrowEntityNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = "user1";
            var categoryId = 999; // Non-existent category

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _categoryService.DeleteCategoryAsync(userId, categoryId));
        }

        [TestMethod]
        public async Task DeleteCategoryAsync_ShouldDeleteCategorySuccessfully()
        {
            // Arrange
            var userId = "user1";
            var categoryId = 1;

            // Act
            await _categoryService.DeleteCategoryAsync(userId, categoryId);

            // Assert
            var deletedCategory = await _categoryService.GetCategoryAsync(categoryId);
            Assert.IsNull(deletedCategory);
        }
    }
}
