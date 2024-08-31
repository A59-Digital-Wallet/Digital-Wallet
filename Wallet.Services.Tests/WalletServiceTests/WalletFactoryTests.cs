using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Factory;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class WalletFactoryTests
    {
        private WalletFactory _walletFactory;

        [TestInitialize]
        public void SetUp()
        {
            _walletFactory = new WalletFactory();
        }

        [TestMethod]
        public void Map_ShouldMapUserWalletRequestToUserWallet()
        {
            // Arrange
            var walletRequest = new UserWalletRequest
            {
                Name = "My Wallet",
                Currency = Currency.USD,
                WalletType = WalletType.Personal
            };

            var overdraftSettings = new OverdraftSettings
            {
                DefaultInterestRate = 0.05m,
                DefaultOverdraftLimit = 1000m
            };

            // Act
            var result = _walletFactory.Map(walletRequest, overdraftSettings);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(walletRequest.Name, result.Name);
            Assert.AreEqual(walletRequest.Currency, result.Currency);
            Assert.AreEqual(0, result.Balance); // Initial balance should be 0
            Assert.AreEqual(walletRequest.WalletType, result.WalletType);
            Assert.AreEqual(overdraftSettings.DefaultInterestRate, result.InterestRate);
            Assert.AreEqual(overdraftSettings.DefaultOverdraftLimit, result.OverdraftLimit);
            Assert.IsNotNull(result.AppUserWallets); // Should be initialized as empty list
        }

        [TestMethod]
        public void Map_ShouldMapUserWalletToWalletDto()
        {
            // Arrange
            var userWallet = new UserWallet
            {
                Id = 1,
                Currency = Currency.EUR,
                Balance = 1500m,
                Name = "Savings Wallet"
            };

            // Act
            var result = _walletFactory.Map(userWallet);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userWallet.Id, result.WalletId);
            Assert.AreEqual(userWallet.Currency, result.Currency);
            Assert.AreEqual(userWallet.Balance, result.Balance);
        }
    }
}
