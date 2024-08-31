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
    public class GetTransactionHistoryContactAsyncTests
    {
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private Mock<IWalletRepository> _mockWalletRepository;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockWalletRepository = new Mock<IWalletRepository>();
            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                _mockWalletRepository.Object,
                null, null, null, null, null, null, null, null);
        }

        [TestMethod]
        public async Task GetTransactionHistoryContactAsync_Should_Return_Transactions()
        {
            // Arrange
            var transactions = new List<Transaction>
        {
            new Transaction { WalletId = 1, RecipientWalletId = 2, Amount = 100 },
            new Transaction { WalletId = 2, RecipientWalletId = 1, Amount = 50 }
        };

            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync("user1"))
                .ReturnsAsync(new List<UserWallet> { new UserWallet { Id = 1 } });

            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync("contact1"))
                .ReturnsAsync(new List<UserWallet> { new UserWallet { Id = 2 } });

            _mockTransactionRepository.Setup(repo => repo.GetTransactionHistoryContactAsync(It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetTransactionHistoryContactAsync("user1", "contact1");

            // Assert
            Assert.AreEqual(2, result.Count());
        }
    }

}
