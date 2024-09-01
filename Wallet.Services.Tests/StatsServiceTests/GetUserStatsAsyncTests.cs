using Moq;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Response;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wallet.Services.Tests.StatsServiceTests
{
    [TestClass]
    public class GetUserStatsAsyncTests
    {
        private Mock<ITransactionService> _mockTransactionService;
        private Mock<IWalletService> _mockWalletService;
        private Mock<ICurrencyExchangeService> _mockCurrencyExchangeService;
        private StatsService _statsService;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionService = new Mock<ITransactionService>();
            _mockWalletService = new Mock<IWalletService>();
            _mockCurrencyExchangeService = new Mock<ICurrencyExchangeService>();

            _statsService = new StatsService(
                _mockTransactionService.Object,
                _mockWalletService.Object,
                _mockCurrencyExchangeService.Object
            );
        }

        [TestMethod]
        public async Task GetUserStatsAsync_Should_Return_Valid_Stats()
        {
            // Arrange
            var userId = "test-user-id";
            var wallets = new List<UserWallet>
    {
        new UserWallet { Id = 1, Currency = Currency.USD, Balance = 1000, Name = "Wallet 1" },
        new UserWallet { Id = 2, Currency = Currency.EUR, Balance = 500, Name = "Wallet 2" }
    };

            var transactions = new List<TransactionDto>
    {
        new TransactionDto { WalletId = 1, TransactionType = TransactionType.Deposit, Amount = 100, OriginalAmount = 100, RecepientWalledId = 1 },
        new TransactionDto { WalletId = 1, TransactionType = TransactionType.Withdraw, Amount = 50, OriginalAmount = 50, RecepientWalledId = null },
        new TransactionDto { WalletId = 1, TransactionType = TransactionType.Transfer, Amount = 200, OriginalAmount = 200, RecepientWalledId = 2 },
        new TransactionDto { WalletId = 2, TransactionType = TransactionType.Transfer, Amount = 150, OriginalAmount = 150, RecepientWalledId = 2 },
    };

            _mockWalletService.Setup(ws => ws.GetUserWalletsAsync(It.IsAny<string>())).ReturnsAsync(wallets);
            _mockTransactionService.Setup(ts => ts.FilterTransactionsAsync(1, int.MaxValue, It.IsAny<TransactionRequestFilter>(), userId))
                                   .ReturnsAsync((transactions, transactions.Count));
            _mockCurrencyExchangeService.Setup(ces => ces.ConvertAsync(It.IsAny<decimal>(), It.IsAny<Currency>(), It.IsAny<Currency>()))
                                        .ReturnsAsync((decimal amount, Currency from, Currency to) => amount * 1.5m);

            // Act
            var result = await _statsService.GetUserStatsAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2250m, result.TotalBalance); // USD and EUR balances converted to BGN
            Assert.AreEqual(2, result.WalletBreakdown.Count);
            Assert.AreEqual(50, result.WalletBreakdown[0].TotalWithdrawals);
            Assert.AreEqual(100, result.WalletBreakdown[0].TotalDeposits);
            Assert.AreEqual(200, result.WalletBreakdown[0].TotalTransfersSent);
            Assert.AreEqual(200, result.WalletBreakdown[1].TotalTransfersReceived);
        }


        [TestMethod]
        public async Task GetBalanceOverTime_Should_Return_Correct_Balance_Over_Time()
        {
            // Arrange
            var walletId = 1;
            var userId = "test-user-id";
            var interval = "daily";
            var transactions = new List<TransactionDto>
    {
        new TransactionDto { WalletId = 1, TransactionType = TransactionType.Deposit, Amount = 100, OriginalAmount = 100, Date = DateTime.UtcNow.AddDays(-10), RecepientWalledId = 1 },
        new TransactionDto { WalletId = 1, TransactionType = TransactionType.Withdraw, Amount = 50, OriginalAmount = 50, Date = DateTime.UtcNow.AddDays(-5), RecepientWalledId = null },
        new TransactionDto { WalletId = 1, TransactionType = TransactionType.Transfer, Amount = 200, OriginalAmount = 200, Date = DateTime.UtcNow.AddDays(-3), RecepientWalledId = 2 },
        new TransactionDto { WalletId = 1, TransactionType = TransactionType.Transfer, Amount = 150, OriginalAmount = 150, Date = DateTime.UtcNow.AddDays(-1), RecepientWalledId = 2 },
    };

            _mockTransactionService.Setup(ts => ts.FilterTransactionsAsync(1, int.MaxValue, It.IsAny<TransactionRequestFilter>(), userId))
                                   .ReturnsAsync((transactions, transactions.Count));

            // Act
            var (labels, balances) = await _statsService.GetBalanceOverTime(walletId, interval, userId);

            // Assert
            Assert.AreEqual(15, labels.Count); // Expecting 14 days of labels + initial balance day
            Assert.AreEqual(-300, balances.Last()); // The last balance should reflect the final balance after all transactions
        }


    }
}