using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class ProcessRecurringTransactionsAsyncTests
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
                null, // Assuming no need for currency exchange service
                null, // Assuming no need for card repository
                null, // Assuming no need for transaction factory
                null, // Assuming no need for user manager
                null, // Assuming no need for verify email service
                null, // Assuming no need for memory cache
                null, // Assuming no need for email sender
                null  // Assuming no need for transaction validator
            );
        }

        [TestMethod]
        public async Task ProcessRecurringTransactionsAsync_Should_Process_Transactions()
        {
            // Arrange
            var recurringTransaction = new Transaction
            {
                Id = 1,
                WalletId = 1,
                Amount = 100,
                Date = DateTime.UtcNow.AddDays(-10),
                TransactionType = TransactionType.Deposit,
                OriginalCurrency = Currency.USD,
                Status = TransactionStatus.Pending,
                IsRecurring = true,
                Interval = RecurrenceInterval.Monthly,
                NextExecutionDate = DateTime.UtcNow.AddDays(-1)
            };

            var lowBalanceTransaction = new Transaction
            {
                Id = 2,
                WalletId = 2,
                Amount = 200,
                Date = DateTime.UtcNow.AddDays(-10),
                TransactionType = TransactionType.Withdraw,
                OriginalCurrency = Currency.USD,
                Status = TransactionStatus.Pending,
                IsRecurring = true,
                Interval = RecurrenceInterval.Monthly,
                NextExecutionDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockTransactionRepository.Setup(repo => repo.GetRecurringTransactionsDueAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Transaction> { recurringTransaction, lowBalanceTransaction });

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(1))
                .ReturnsAsync(new UserWallet { Id = 1, Balance = 200, WalletType = WalletType.Personal });

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(2))
                .ReturnsAsync(new UserWallet { Id = 2, Balance = 100, WalletType = WalletType.Personal });

            // Act
            await _transactionService.ProcessRecurringTransactionsAsync();

            // Assert
            _mockTransactionRepository.Verify(repo => repo.UpdateTransactionAsync(It.Is<Transaction>(t => t.Id == 1 && t.LastExecutedDate.HasValue)), Times.Once);
            _mockTransactionRepository.Verify(repo => repo.UpdateTransactionAsync(It.Is<Transaction>(t => t.Id == 2 && !t.LastExecutedDate.HasValue)), Times.Once);
        }
    }
}
