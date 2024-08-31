using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Factory;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class CategoryFactoryTests
    {
        private CategoryFactory _categoryFactory;

        [TestInitialize]
        public void Setup()
        {
            _categoryFactory = new CategoryFactory();
        }

        [TestMethod]
        public void Map_ShouldMapCategoryToCategoryResponseDTO_WithTransactions()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Food",
                Transactions = new List<Transaction>
                {
                new Transaction
                {
                    Id = 1,
                    WalletId = 1,
                    RecipientWalletId = 2,
                    Amount = 100,
                    Date = DateTime.UtcNow.AddDays(-10),
                    TransactionType = TransactionType.Deposit,
                    OriginalCurrency = Wallet.Data.Models.Enums.Currency.USD,
                },
                }
            };

            // Act
            var result = _categoryFactory.Map(new List<Category> { category });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(category.Id, result[0].Id);
            Assert.AreEqual(category.Name, result[0].Name);
            Assert.AreEqual(1, result[0].Transactions.Count);
            Assert.AreEqual(category.Transactions.First().OriginalAmount, result[0].Transactions.First().Amount);
            Assert.AreEqual(category.Transactions.First().Description, result[0].Transactions.First().Description);
        }

        [TestMethod]
        public void Map_ShouldMapCategoryToCategoryResponseDTO_WithoutTransactions()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Transport",
                Transactions = null // No transactions
            };

            // Act
            var result = _categoryFactory.Map(new List<Category> { category });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(category.Id, result[0].Id);
            Assert.AreEqual(category.Name, result[0].Name);
            Assert.AreEqual(0, result[0].Transactions.Count); // Empty list
        }

        [TestMethod]
        public void Map_ShouldMapCategoryRequestDTOToCategory()
        {
            // Arrange
            var userId = "user1";
            var categoryRequest = new CategoryRequestDTO
            {
                Name = "New Category"
            };

            // Act
            var result = _categoryFactory.Map(userId, categoryRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result.UserId);
            Assert.AreEqual(categoryRequest.Name, result.Name);
        }

        [TestMethod]
        public void Map_ShouldMapCategoryRequestDTOToExistingCategory()
        {
            // Arrange
            var userId = "user1";
            var categoryId = 1;
            var categoryRequest = new CategoryRequestDTO
            {
                Name = "Updated Category"
            };

            // Act
            var result = _categoryFactory.Map(userId, categoryId, categoryRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(categoryId, result.Id);
            Assert.AreEqual(userId, result.UserId);
            Assert.AreEqual(categoryRequest.Name, result.Name);
        }

        [TestMethod]
        public void Map_ShouldMapSingleCategoryToCategoryResponseDTO()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Utilities"
            };

            // Act
            var result = _categoryFactory.Map(category);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(category.Name, result.Name);
        }
    }
}
