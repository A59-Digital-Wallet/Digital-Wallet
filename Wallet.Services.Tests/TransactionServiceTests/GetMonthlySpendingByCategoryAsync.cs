using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class GetMonthlySpendingByCategoryAsyncTests
    {
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                null, null, null, null, null, null, null, null, null);
        }

        [TestMethod]
        public async Task GetMonthlySpendingByCategoryAsync_Should_Return_Spending_By_Category()
        {
            // Arrange
            var transactions = new List<Transaction>
        {
            new Transaction { Date = DateTime.UtcNow, Amount = 100, TransactionType = TransactionType.Withdraw, Category = new Category { Name = "Food" } },
            new Transaction { Date = DateTime.UtcNow, Amount = 50, TransactionType = TransactionType.Transfer, Category = new Category { Name = "Transport" } }
        };

            _mockTransactionRepository.Setup(repo => repo.GetTransactionsByWalletId(1))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetMonthlySpendingByCategoryAsync("user1", 1);

            // Assert
            Assert.AreEqual(2, result.Count); // 2 categories
            Assert.AreEqual(100, result["Food"]);
            Assert.AreEqual(50, result["Transport"]);
        }
    }

}
