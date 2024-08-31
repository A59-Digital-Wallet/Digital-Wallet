using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Factory;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class TransactionFactoryTests
    {
        private TransactionFactory _transactionFactory;

        [TestInitialize]
        public void SetUp()
        {
            _transactionFactory = new TransactionFactory();
        }

        [TestMethod]
        public void Map_ShouldMapTransactionRequestToTransaction()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel
            {
                Amount = 100.50m,
                WalletId = 1,
                Description = "Test transaction",
                TransactionType = TransactionType.Transfer,
                IsRecurring = true,
                RecurrenceInterval = RecurrenceInterval.Monthly,
                RecepientWalletId = 2,
                CardId = 5
            };

            // Act
            var result = _transactionFactory.Map(transactionRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transactionRequest.Amount, result.Amount);
            Assert.AreEqual(transactionRequest.WalletId, result.WalletId);
            Assert.AreEqual(transactionRequest.Description, result.Description);
            Assert.AreEqual(TransactionStatus.Pending, result.Status);
            Assert.AreEqual(transactionRequest.TransactionType, result.TransactionType);
            Assert.AreEqual(transactionRequest.IsRecurring, result.IsRecurring);
            Assert.AreEqual(transactionRequest.RecurrenceInterval, result.Interval);
            Assert.AreEqual(transactionRequest.RecepientWalletId, result.RecipientWalletId);
            Assert.IsNull(result.CardId);  // For Transfer, CardId should not be set
            Assert.IsNull(result.CategoryId);  // CategoryId should be null by default
            Assert.AreEqual(DateTime.UtcNow.Date, result.Date.Date);  // Ensure the date is set to today's date
        }

        [TestMethod]
        public void Map_ShouldMapTransactionRequestToTransaction_WithCard()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel
            {
                Amount = 200.75m,
                WalletId = 1,
                Description = "Deposit transaction",
                TransactionType = TransactionType.Deposit,
                IsRecurring = false,
                CardId = 5
            };

            // Act
            var result = _transactionFactory.Map(transactionRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transactionRequest.Amount, result.Amount);
            Assert.AreEqual(transactionRequest.WalletId, result.WalletId);
            Assert.AreEqual(transactionRequest.Description, result.Description);
            Assert.AreEqual(TransactionStatus.Pending, result.Status);
            Assert.AreEqual(transactionRequest.TransactionType, result.TransactionType);
            Assert.AreEqual(transactionRequest.CardId, result.CardId);  // CardId should be set for Deposit
            Assert.IsNull(result.RecipientWalletId);  // For Deposit, RecipientWalletId should not be set
            Assert.IsNull(result.CategoryId);  // CategoryId should be null by default
        }

        [TestMethod]
        public void Map_ShouldMapTransactionToTransactionDto()
        {
            // Arrange
            var transaction = new Transaction
            {
                Id = 1,
                Amount = 300.50m,
                Date = DateTime.UtcNow,
                Description = "Mapped transaction",
                Status = TransactionStatus.Completed,
                WalletId = 1,
                Wallet = new Wallet.Data.Models.UserWallet { Name = "Main Wallet" },
                TransactionType = TransactionType.Withdraw,
                RecipientWalletId = 2,
                RecipientWallet = new Wallet.Data.Models.UserWallet { Name = "Recipient Wallet" },
                IsRecurring = true,
                Interval = RecurrenceInterval.Weekly,
                OriginalAmount = 300.50m,
                OriginalCurrency = Currency.USD,
                SentCurrency = Currency.EUR
            };

            // Act
            var result = _transactionFactory.Map(transaction);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(transaction.Id, result.Id);
            Assert.AreEqual(transaction.Amount, result.Amount);
            Assert.AreEqual(transaction.Date, result.Date);
            Assert.AreEqual(transaction.Description, result.Description);
            Assert.AreEqual(transaction.Status, result.Status);
            Assert.AreEqual(transaction.WalletId, result.WalletId);
            Assert.AreEqual(transaction.Wallet.Name, result.WalletName);
            Assert.AreEqual(transaction.TransactionType, result.TransactionType);
            Assert.AreEqual(transaction.RecipientWalletId, result.RecepientWalledId);
            Assert.AreEqual(transaction.RecipientWallet.Name, result.RecepientWalledName);
            Assert.AreEqual(transaction.IsRecurring, result.IsReccuring);
            Assert.AreEqual(transaction.Interval, result.RecurrenceInterval);
            Assert.AreEqual(transaction.OriginalAmount, result.OriginalAmount);
            Assert.AreEqual(transaction.OriginalCurrency.ToString(), result.OriginalCurrency);
            Assert.AreEqual(transaction.SentCurrency.ToString(), result.SentCurrency);
        }
    }
}
