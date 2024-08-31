using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class GetWeeklySpendingAsyncTests
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
        public async Task GetWeeklySpendingAsync_Should_Return_Weekly_Spending()
        {
            // Arrange
            var transactions = new List<Transaction>
        {
            new Transaction { Date = DateTime.UtcNow.AddDays(-1), OriginalAmount = 100, TransactionType = TransactionType.Withdraw },
            new Transaction { Date = DateTime.UtcNow.AddDays(-2), OriginalAmount = 50, TransactionType = TransactionType.Transfer }
        };

            _mockTransactionRepository.Setup(repo => repo.GetTransactionsByWalletId(1))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetWeeklySpendingAsync(1);

            // Assert
            Assert.AreEqual(5, result.WeekLabels.Count); // Assuming max 5 weeks
            Assert.IsTrue(result.WeeklySpendingAmounts.All(amount => amount >= 0)); // Spending should be non-negative
        }
    }

}
