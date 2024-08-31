using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Implementations;
using Wallet.Services.Contracts;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Factory.Contracts;
using Microsoft.AspNetCore.Identity;
using Wallet.DTO.Response;
using Wallet.Data.Models.Transactions;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class FilterTransactionsAsyncTests
    {
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<ITransactionFactory> _mockTransactionFactory;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockWalletRepository = new Mock<IWalletRepository>();
            _mockTransactionFactory = new Mock<ITransactionFactory>();

            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                _mockWalletRepository.Object,
                null, // No currency exchange needed
                null, // No card repository needed
                _mockTransactionFactory.Object,
                _mockUserManager.Object,
                null, // No verify email service needed
                null, // No cache needed
                null, // No email sender needed
                null // No transaction validator needed
            );
        }

        [TestMethod]
        public async Task FilterTransactionsAsync_Should_Return_Filtered_Transactions()
        {
            // Arrange
            var userId = "test-user-id";
            var filter = new TransactionRequestFilter();
            var transactions = new List<Transaction> { new Transaction { Id = 1, WalletId = 1 } };
            var wallets = new List<UserWallet> { new UserWallet { Id = 1 } };
            var user = new AppUser { Id = userId, LastSelectedWalletId = 1 };

            _mockTransactionRepository.Setup(repo => repo.FilterBy(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TransactionRequestFilter>(), It.IsAny<string>()))
                                      .ReturnsAsync(transactions);

            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync(It.IsAny<string>()))
                                 .ReturnsAsync(wallets);

            _mockUserManager.Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync(user);

            _mockTransactionFactory.Setup(factory => factory.Map(It.IsAny<Transaction>()))
                                   .Returns(new TransactionDto());

            // Act
            var result = await _transactionService.FilterTransactionsAsync(1, 10, filter, userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}
