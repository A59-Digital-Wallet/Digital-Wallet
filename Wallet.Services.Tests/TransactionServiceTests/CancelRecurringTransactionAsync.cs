using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Implementations;
using Wallet.Services.Contracts;
using Wallet.Data.Repositories.Contracts;
using Microsoft.AspNetCore.Identity;
using Wallet.Data.Models.Transactions;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class CancelRecurringTransactionAsyncTests
    {
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockWalletRepository = new Mock<IWalletRepository>();

            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                _mockWalletRepository.Object,
                null, // No currency exchange needed
                null, // No card repository needed
                null, // No transaction factory needed
                _mockUserManager.Object,
                null, // No verify email service needed
                null, // No cache needed
                null, // No email sender needed
                null // No transaction validator needed
            );
        }

        [TestMethod]
        public async Task CancelRecurringTransactionAsync_Should_Cancel_Recurring_Transaction()
        {
            // Arrange
            var transactionId = 1;
            var userId = "test-user-id";
            var transaction = new Transaction
            {
                Id = transactionId,
                Wallet = new UserWallet { OwnerId = userId },
                IsRecurring = true
            };

            _mockTransactionRepository.Setup(repo => repo.GetTransactionByIdAsync(It.IsAny<int>()))
                                      .ReturnsAsync(transaction);

            // Act
            await _transactionService.CancelRecurringTransactionAsync(transactionId, userId);

            // Assert
            _mockTransactionRepository.Verify(repo => repo.UpdateTransactionAsync(It.Is<Transaction>(t => t.IsRecurring == false && t.IsActive == false)), Times.Once);
        }
    }
}
