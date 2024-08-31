using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Implementations;
using Microsoft.AspNetCore.Identity;

namespace Wallet.Services.Tests.WalletServiceTests
{
    [TestClass]
    public class WalletServiceTests
    {
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private Mock<IWalletFactory> _mockWalletFactory;
        private Mock<IOverdraftSettingsRepository> _mockOverdraftSettingsRepository;
        private WalletService _walletService;

        [TestInitialize]
        public void Setup()
        {
            _mockWalletRepository = new Mock<IWalletRepository>();
            _mockWalletFactory = new Mock<IWalletFactory>();
            _mockOverdraftSettingsRepository = new Mock<IOverdraftSettingsRepository>();

            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            _walletService = new WalletService(
                _mockWalletRepository.Object,
                _mockUserManager.Object,
                _mockWalletFactory.Object,
                _mockOverdraftSettingsRepository.Object
            );
        }

        [TestMethod]
        public async Task AddMemberToJointWalletAsync_Should_Add_Member_When_Wallet_Is_Joint()
        {
            // Arrange
            var walletId = 1;
            var userId = "user2";
            var ownerId = "owner1";

            var wallet = new UserWallet
            {
                Id = walletId,
                WalletType = WalletType.Joint,
                OwnerId = ownerId,
                AppUserWallets = new List<AppUser>()
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            var ownerUser = new AppUser { Id = ownerId };
            var user = new AppUser { Id = userId };

            _mockUserManager.Setup(um => um.FindByIdAsync(ownerId))
                .ReturnsAsync(ownerUser);
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _walletService.AddMemberToJointWalletAsync(walletId, userId, true, true, ownerId);

            // Assert
            Assert.IsTrue(user.JointWallets.Contains(wallet));
            _mockWalletRepository.Verify(repo => repo.AddMemberToJointWalletAsync(walletId, ownerUser), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public async Task AddMemberToJointWalletAsync_Should_Throw_When_Wallet_Is_Not_Joint()
        {
            // Arrange
            var walletId = 1;
            var userId = "user2";
            var ownerId = "owner1";

            var wallet = new UserWallet
            {
                Id = walletId,
                WalletType = WalletType.Personal,
                OwnerId = ownerId
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            // Act
            await _walletService.AddMemberToJointWalletAsync(walletId, userId, true, true, ownerId);

            // Assert: Exception is expected, so no additional assertions needed
        }

        [TestMethod]
        public async Task CreateWallet_Should_Create_Wallet_With_Valid_Currency()
        {
            // Arrange
            var walletRequest = new UserWalletRequest
            {
                Currency = Currency.BGN,
                Name = "Test Wallet"
            };
            var userId = "user1";

            var overdraftSettings = new OverdraftSettings();
            _mockOverdraftSettingsRepository.Setup(repo => repo.GetSettingsAsync())
                .ReturnsAsync(overdraftSettings);

            var mappedWallet = new UserWallet
            {
                Name = walletRequest.Name,
                Currency = walletRequest.Currency,
                OwnerId = userId
            };
            _mockWalletFactory.Setup(factory => factory.Map(walletRequest, overdraftSettings))
                .Returns(mappedWallet);

            // Act
            await _walletService.CreateWallet(walletRequest, userId);

            // Assert
            _mockWalletRepository.Verify(repo => repo.CreateWallet(mappedWallet), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateWallet_Should_Throw_When_Currency_Is_None()
        {
            // Arrange
            var walletRequest = new UserWalletRequest
            {
                Currency = Currency.None,
                Name = "Test Wallet"
            };
            var userId = "user1";

            // Act
            await _walletService.CreateWallet(walletRequest, userId);

            // Assert: Exception is expected, so no additional assertions needed
        }

        [TestMethod]
        public async Task GetWalletAsync_Should_Return_Wallet_When_User_Is_Owner_Or_Member()
        {
            // Arrange
            var walletId = 1;
            var userId = "user1";

            var wallet = new UserWallet
            {
                Id = walletId,
                OwnerId = userId,
                Balance = 1000,
                Currency = Currency.BGN,
                Name = "Test Wallet",
                WalletType = WalletType.Personal,
                AppUserWallets = new List<AppUser>()
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            // Act
            var result = await _walletService.GetWalletAsync(walletId, userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(wallet.Balance, result.Balance);
            Assert.AreEqual(wallet.Currency, result.Currency);
            Assert.AreEqual(wallet.Name, result.Name);
            _mockWalletRepository.Verify(repo => repo.GetWalletAsync(walletId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public async Task GetWalletAsync_Should_Throw_When_User_Is_Not_Owner_Or_Member()
        {
            // Arrange
            var walletId = 1;
            var userId = "user2"; // Not the owner

            var wallet = new UserWallet
            {
                Id = walletId,
                OwnerId = "owner1", // Different owner
                AppUserWallets = new List<AppUser>()
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            // Act
            await _walletService.GetWalletAsync(walletId, userId);

            // Assert: Exception is expected, so no additional assertions needed
        }

        [TestMethod]
        public async Task ToggleOverdraftAsync_Should_Enable_Or_Disable_Overdraft()
        {
            // Arrange
            var walletId = 1;
            var userId = "user1";

            var wallet = new UserWallet
            {
                Id = walletId,
                OwnerId = userId,
                WalletType = WalletType.Personal,
                IsOverdraftEnabled = false
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            _mockWalletRepository.Setup(repo => repo.UpdateWalletAsync())
                .ReturnsAsync(true);

            // Act
            await _walletService.ToggleOverdraftAsync(walletId, userId);

            // Assert
            Assert.IsTrue(wallet.IsOverdraftEnabled);
            _mockWalletRepository.Verify(repo => repo.UpdateWalletAsync(), Times.Once);
        }
    }
}
