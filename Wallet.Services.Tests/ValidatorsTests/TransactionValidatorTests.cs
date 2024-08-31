using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Validation.TransactionValidation;

namespace Wallet.Services.Tests.ValidatorsTests
{
    [TestClass]
    public class TransactionValidatorTests
    {
        private TransactionValidator _transactionValidator;

        [TestInitialize]
        public void Setup()
        {
            _transactionValidator = new TransactionValidator();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateOverdraftAndBalance_ShouldThrowException_WhenTransactionExceedsOverdraftLimit()
        {
            // Arrange
            var wallet = new UserWallet
            {
                Balance = 1000m,
                OverdraftLimit = 500m,
                IsOverdraftEnabled = true,
                WalletType = WalletType.Personal
            };
            var transactionAmount = 1600m; // Exceeds the balance + overdraft

            // Act
            _transactionValidator.ValidateOverdraftAndBalance(wallet, transactionAmount);
        }

        [TestMethod]
        public void ValidateOverdraftAndBalance_ShouldNotThrowException_WhenTransactionIsWithinOverdraftLimit()
        {
            // Arrange
            var wallet = new UserWallet
            {
                Balance = 1000m,
                OverdraftLimit = 500m,
                IsOverdraftEnabled = true,
                WalletType = WalletType.Personal
            };
            var transactionAmount = 1200m; // Within the balance + overdraft

            // Act
            _transactionValidator.ValidateOverdraftAndBalance(wallet, transactionAmount);

            // Assert
            // No exception should be thrown
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateOverdraftAndBalance_ShouldThrowException_WhenTransactionExceedsBalanceWithoutOverdraft()
        {
            // Arrange
            var wallet = new UserWallet
            {
                Balance = 1000m,
                IsOverdraftEnabled = false,
                WalletType = WalletType.Personal
            };
            var transactionAmount = 1500m; // Exceeds the balance without overdraft

            // Act
            _transactionValidator.ValidateOverdraftAndBalance(wallet, transactionAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public void ValidateWalletOwnership_ShouldThrowException_WhenUserDoesNotOwnWallet()
        {
            // Arrange
            var wallet = new UserWallet
            {
                OwnerId = "owner-1",
                AppUserWallets = new List<AppUser>
                {
                    new AppUser { Id = "user-1" }
                }
            };
            var userId = "user-2"; // User does not own the wallet and is not associated

            // Act
            _transactionValidator.ValidateWalletOwnership(wallet, userId);
        }

        [TestMethod]
        public void ValidateWalletOwnership_ShouldNotThrowException_WhenUserOwnsWallet()
        {
            // Arrange
            var wallet = new UserWallet
            {
                OwnerId = "owner-1",
                AppUserWallets = new List<AppUser>
                {
                    new AppUser { Id = "user-1" }
                }
            };
            var userId = "owner-1"; // User is the owner of the wallet

            // Act
            _transactionValidator.ValidateWalletOwnership(wallet, userId);

            // Assert
            // No exception should be thrown
        }

        [TestMethod]
        public void IsHighValueTransaction_ShouldReturnTrue_WhenTransactionIsHighValue()
        {
            // Arrange
            var wallet = new UserWallet
            {
                Balance = 1000m
            };
            var transactionRequest = new TransactionRequestModel
            {
                Amount = 900m, // 90% of the balance
                TransactionType = TransactionType.Withdraw
            };

            // Act
            var result = _transactionValidator.IsHighValueTransaction(transactionRequest, wallet);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsHighValueTransaction_ShouldReturnFalse_WhenTransactionIsNotHighValue()
        {
            // Arrange
            var wallet = new UserWallet
            {
                Balance = 1000m
            };
            var transactionRequest = new TransactionRequestModel
            {
                Amount = 100m, // 10% of the balance
                TransactionType = TransactionType.Withdraw
            };

            // Act
            var result = _transactionValidator.IsHighValueTransaction(transactionRequest, wallet);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsHighValueTransaction_ShouldReturnFalse_WhenTransactionTypeIsDeposit()
        {
            // Arrange
            var wallet = new UserWallet
            {
                Balance = 1000m
            };
            var transactionRequest = new TransactionRequestModel
            {
                Amount = 20000m, // High value but a deposit
                TransactionType = TransactionType.Deposit
            };

            // Act
            var result = _transactionValidator.IsHighValueTransaction(transactionRequest, wallet);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
