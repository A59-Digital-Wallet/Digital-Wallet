using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Implementations;
using Wallet.Services.Contracts;
using Wallet.Data.Repositories.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Wallet.DTO.Request;
using Wallet.Data.Models.Enums;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Validation.TransactionValidation;
using Wallet.Data.Models.Transactions;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class VerifyTransactionAsyncTests
    {
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private Mock<IMemoryCache> _mockMemoryCache;
        private Mock<ITransactionFactory> _mockTransactionFactory;
        private Mock<ITransactionValidator> _mockTransactionValidator;
        private Mock<ICurrencyExchangeService> _mockCurrencyExchangeService;
        private TransactionService _transactionService;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockWalletRepository = new Mock<IWalletRepository>();

            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockTransactionFactory = new Mock<ITransactionFactory>();
            _mockTransactionValidator = new Mock<ITransactionValidator>();
            _mockCurrencyExchangeService = new Mock<ICurrencyExchangeService>();

            _transactionService = new TransactionService(
                _mockTransactionRepository.Object,
                _mockWalletRepository.Object,
                _mockCurrencyExchangeService.Object,
                null, // No card repository needed
                _mockTransactionFactory.Object,
                _mockUserManager.Object,
                null, // No verify email service needed
                _mockMemoryCache.Object,
                null, // No email sender needed
                _mockTransactionValidator.Object
            );
        }

        [TestMethod]
        public async Task VerifyTransactionAsync_Should_Return_True_For_Valid_Token_And_Code()
        {
            // Arrange
            var token = "validToken";
            var verificationCode = "123456";
            var userId = "user1";
            var walletId = 1;
            var recipientWalletId = 2;
            var walletBalance = 1000m;
            var transferAmount = walletBalance * 0.85m;  // 85% of the wallet balance, so it's a high-value transaction

            var transactionRequest = new TransactionRequestModel
            {
                WalletId = walletId,
                Amount = transferAmount,
                TransactionType = TransactionType.Transfer,
                RecepientWalletId = recipientWalletId
            };

            var pendingTransaction = new PendingTransaction
            {
                TransactionRequest = transactionRequest,
                Wallet = new UserWallet { Id = walletId, OwnerId = userId, Balance = walletBalance },
                UserId = userId,
                VerificationCode = verificationCode
            };

            object cacheEntry = pendingTransaction;
            _mockMemoryCache
                .Setup(cache => cache.TryGetValue(It.IsAny<string>(), out cacheEntry))
                .Returns(true);

            _mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new AppUser { Id = userId, EmailConfirmationCode = verificationCode, EmailConfirmationCodeGeneratedAt = DateTime.UtcNow });

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(new UserWallet { Id = walletId, OwnerId = userId, Balance = walletBalance });

            // Mock the recipient wallet
            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(recipientWalletId))
                .ReturnsAsync(new UserWallet { Id = recipientWalletId, OwnerId = "recipientUserId", Balance = 500 });

            _mockTransactionFactory.Setup(factory => factory.Map(It.IsAny<TransactionRequestModel>()))
                .Returns(new Transaction { WalletId = walletId, Amount = transferAmount });

            _mockTransactionValidator.Setup(v => v.IsHighValueTransaction(It.IsAny<TransactionRequestModel>(), It.IsAny<UserWallet>()))
                .Returns(true); // Indicate that this is a high-value transaction

            _mockTransactionRepository.Setup(repo => repo.CreateTransactionAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            _mockWalletRepository.Setup(repo => repo.UpdateWalletAsync())
                .ReturnsAsync(true); // Ensure the wallet update operation succeeds

            // Act
            var result = await _transactionService.VerifyTransactionAsync(token, verificationCode);

            // Assert
            Assert.IsTrue(result);
        }

    }
}
