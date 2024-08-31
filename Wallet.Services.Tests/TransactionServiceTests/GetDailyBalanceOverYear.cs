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
    public class GetDailyBalanceOverYearTests
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
        public async Task GetDailyBalanceOverYear_Should_Return_Daily_Balances()
        {
            // Arrange
            var transactions = new List<Transaction>
        {
            new Transaction { Date = DateTime.UtcNow.AddMonths(-2), OriginalAmount = 100, TransactionType = TransactionType.Deposit },
            new Transaction { Date = DateTime.UtcNow.AddMonths(-1), OriginalAmount = 50, TransactionType = TransactionType.Withdraw }
        };

            _mockTransactionRepository.Setup(repo => repo.GetTransactionsByWalletId(1))
                .ReturnsAsync(transactions);

            var wallet = new UserWallet { Id = 1, Balance = 1000 };
            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(1))
                .ReturnsAsync(wallet);

            // Calculate the expected number of days
            var now = DateTime.UtcNow;
            var oneYearAgo = now.AddYears(-1);
            var expectedDays = (now - oneYearAgo).Days + 1;

            // Act
            var result = await _transactionService.GetDailyBalanceOverYear(1);

            // Assert
            Assert.AreEqual(expectedDays, result.Item1.Count); // Number of days in the year
            Assert.IsTrue(result.Item2.All(balance => balance >= 0)); // Balance should be non-negative
        }
    }


}
