using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.Services.Implementations;
using Wallet.Services.Validation.TransactionValidation;
using Wallet.Services.Contracts;
using Wallet.Data.Repositories.Contracts;
using Microsoft.AspNetCore.Identity;
using Wallet.Services.Factory.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Wallet.Common.Exceptions;
using Wallet.Data.Models.Transactions;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class CreateTransactionAsyncTests
    {
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<ICurrencyExchangeService> _mockCurrencyExchangeService;
        private Mock<ICardRepository> _mockCardRepository;
        private Mock<ITransactionFactory> _mockTransactionFactory;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private Mock<VerifyEmailService> _mockVerifyEmailService;
        private Mock<IMemoryCache> _mockTransactionCache;
        private Mock<IEmailSender> _mockEmailSender;
        private Mock<ITransactionValidator> _mockTransactionValidator;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockWalletRepository = new Mock<IWalletRepository>();
            _mockCurrencyExchangeService = new Mock<ICurrencyExchangeService>();
            _mockCardRepository = new Mock<ICardRepository>();
            _mockTransactionFactory = new Mock<ITransactionFactory>();

            // Initialize the UserManager mock correctly
            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            // Create the VerifyEmailService mock correctly by passing a mock of IEmailSender
            _mockEmailSender = new Mock<IEmailSender>();
            _mockVerifyEmailService = new Mock<VerifyEmailService>(_mockEmailSender.Object);

            // Initialize the IMemoryCache mock correctly
            _mockTransactionCache = new Mock<IMemoryCache>();

            // Initialize the ITransactionValidator mock
            _mockTransactionValidator = new Mock<ITransactionValidator>();

            _mockTransactionCache = new Mock<IMemoryCache>();
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockTransactionCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);
            // Now, create the TransactionService object with all dependencies
            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                _mockWalletRepository.Object,
                _mockCurrencyExchangeService.Object,
                _mockCardRepository.Object,
                _mockTransactionFactory.Object,
                _mockUserManager.Object,
                _mockVerifyEmailService.Object,
                _mockTransactionCache.Object,
                _mockEmailSender.Object,
                _mockTransactionValidator.Object
            );
        }

        [TestMethod]
        public async Task CreateTransactionAsync_Should_Call_ValidateWalletOwnership()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel
            {
                WalletId = 1,
                Amount = 100,
                TransactionType = TransactionType.Withdraw // Ensure it's Withdraw or Transfer
            };

            var user = new AppUser { Id = "user1", UserName = "testuser" };

            var wallet = new UserWallet
            {
                Id = 1,
                OwnerId = "user1",
                Balance = 1000,
                WalletType = WalletType.Personal
            };

            _mockWalletRepository
                .Setup(r => r.GetWalletAsync(It.IsAny<int>()))
                .ReturnsAsync(wallet);

            _mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockTransactionValidator
                .Setup(v => v.ValidateOverdraftAndBalance(It.IsAny<UserWallet>(), It.IsAny<decimal>()))
                .Verifiable();

            _mockTransactionFactory
                .Setup(f => f.Map(It.IsAny<TransactionRequestModel>()))
                .Returns(new Transaction());

            // Act
            await _transactionService.CreateTransactionAsync(transactionRequest, "user1");

            // Assert
            _mockTransactionValidator
                .Verify(v => v.ValidateOverdraftAndBalance(It.IsAny<UserWallet>(), It.IsAny<decimal>()), Times.Once);
        }


        [TestMethod]
        public async Task CreateTransactionAsync_Should_Throw_Exception_If_Wallet_Not_Found()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel
            {
                WalletId = 1,
                Amount = 100,
                TransactionType = TransactionType.Deposit
            };
            var userId = "test-user-id";

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(It.IsAny<int>()))
                                 .ReturnsAsync((UserWallet)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _transactionService.CreateTransactionAsync(transactionRequest, userId));
        }

        [TestMethod]
        public async Task CreateTransactionAsync_Should_Handle_HighValueTransaction()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel
            {
                WalletId = 1,
                Amount = 30000,
                TransactionType = TransactionType.Transfer,
                RecepientWalletId = 2
            };

            var wallet = new UserWallet
            {
                Id = 1,
                Balance = 10000,
                WalletType = WalletType.Personal,
                OwnerId = "user1"
            };

            var user = new AppUser
            {
                Id = "user1",
                Email = "user@example.com",
                UserName = "user1"
            };

            _mockWalletRepository
                .Setup(r => r.GetWalletAsync(It.IsAny<int>()))
                .ReturnsAsync(wallet);

            _mockUserManager
                .Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockTransactionValidator
                .Setup(v => v.IsHighValueTransaction(It.IsAny<TransactionRequestModel>(), It.IsAny<UserWallet>()))
                .Returns(true);

            // Mocking the Set method of IMemoryCache
            _mockTransactionCache = new Mock<IMemoryCache>();
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockTransactionCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<VerificationRequiredException>(() =>
                _transactionService.CreateTransactionAsync(transactionRequest, user.Id));
        }

    }
}
