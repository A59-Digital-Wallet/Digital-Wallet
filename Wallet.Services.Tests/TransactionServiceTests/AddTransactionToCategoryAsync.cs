using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class AddTransactionToCategoryAsyncTests
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
        public async Task AddTransactionToCategoryAsync_Should_Update_Transaction_With_Category()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = 1,
                Wallet = new UserWallet { Id = 1, OwnerId = "user1" }
            };

            _mockTransactionRepository.Setup(repo => repo.GetTransactionByIdAsync(1))
                .ReturnsAsync(transaction);

            _mockTransactionRepository.Setup(repo => repo.UpdateTransactionAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            // Act
            await _transactionService.AddTransactionToCategoryAsync(1, 2, "user1");

            // Assert
            Assert.AreEqual(2, transaction.CategoryId);
            _mockTransactionRepository.Verify(repo => repo.UpdateTransactionAsync(transaction), Times.Once);
        }
    }

}
