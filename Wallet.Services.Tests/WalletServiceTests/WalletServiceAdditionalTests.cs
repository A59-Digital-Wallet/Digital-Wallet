using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Implementations;
using Microsoft.AspNetCore.Identity;

namespace Wallet.Services.Tests.WalletServiceTests
{
    [TestClass]
    public class WalletServiceAdditionalTests
    {
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private WalletService _walletService;

        [TestInitialize]
        public void Setup()
        {
            _mockWalletRepository = new Mock<IWalletRepository>();

            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            _walletService = new WalletService(
                _mockWalletRepository.Object,
                _mockUserManager.Object,
                null, // No wallet factory needed
                null  // No overdraft settings repository needed
            );
        }

        [TestMethod]
        public async Task GetWalletMembersAsync_Should_Return_Members_When_Wallet_Exists()
        {
            // Arrange
            var walletId = 1;
            var appUsers = new List<AppUser>
            {
                new AppUser { Id = "user1" },
                new AppUser { Id = "user2" }
            };
            var wallet = new UserWallet
            {
                Id = walletId,
                AppUserWallets = appUsers
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            // Act
            var result = await _walletService.GetWalletMembersAsync(walletId);

            // Assert
            Assert.AreEqual(appUsers.Count, result.Count);
            CollectionAssert.AreEquivalent(appUsers, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetWalletMembersAsync_Should_Throw_When_Wallet_Does_Not_Exist()
        {
            // Arrange
            var walletId = 1;

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync((UserWallet)null);

            // Act
            await _walletService.GetWalletMembersAsync(walletId);

            // Assert: Exception is expected
        }

        [TestMethod]
        public async Task GetUserWalletsAsync_Should_Return_Wallets_For_User()
        {
            // Arrange
            var userId = "user1";
            var userWallets = new List<UserWallet>
            {
                new UserWallet { Id = 1 },
                new UserWallet { Id = 2 }
            };

            _mockWalletRepository.Setup(repo => repo.GetUserWalletsAsync(userId))
                .ReturnsAsync(userWallets);

            // Act
            var result = await _walletService.GetUserWalletsAsync(userId);

            // Assert
            Assert.AreEqual(userWallets.Count, result.Count);
            CollectionAssert.AreEquivalent(userWallets, result);
        }

        [TestMethod]
        public async Task GetWalletsForProcessingAsync_Should_Return_Wallets_For_Processing()
        {
            // Arrange
            var walletsForProcessing = new List<UserWallet>
            {
                new UserWallet { Id = 1 },
                new UserWallet { Id = 2 }
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletsForProcessingAsync())
                .ReturnsAsync(walletsForProcessing);

            // Act
            var result = await _walletService.GetWalletsForProcessingAsync();

            // Assert
            Assert.AreEqual(walletsForProcessing.Count, result.Count);
            CollectionAssert.AreEquivalent(walletsForProcessing, result);
        }

        [TestMethod]
        public async Task RemoveMemberFromJointWalletAsync_Should_Remove_Member_When_Conditions_Are_Met()
        {
            // Arrange
            var walletId = 1;
            var userId = "user1"; // This is the member to be removed, not the owner
            var ownerId = "owner1";
            var appUserWallet = new AppUser { Id = userId };
            var ownerUser = new AppUser { Id = ownerId };
            var wallet = new UserWallet
            {
                Id = walletId,
                OwnerId = ownerId,
                AppUserWallets = new List<AppUser> { appUserWallet },
                WalletType = WalletType.Joint// Including the owner and the member
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(appUserWallet);
            appUserWallet.JointWallets.Add(wallet);

            _mockWalletRepository.Setup(repo => repo.RemoveMemberFromJointWalletAsync(walletId, appUserWallet))
        .Callback<int, AppUser>((wId, user) => wallet.AppUserWallets.Remove(user)) // Simulate removal
        .Returns(Task.CompletedTask);


            // Act
            await _walletService.RemoveMemberFromJointWalletAsync(walletId, userId, ownerId);

            // Assert
            Assert.IsFalse(wallet.AppUserWallets.Contains(appUserWallet));
            _mockWalletRepository.Verify(repo => repo.RemoveMemberFromJointWalletAsync(walletId, appUserWallet), Times.Once);
        }


        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public async Task RemoveMemberFromJointWalletAsync_Should_Throw_When_User_Is_Not_Owner()
        {
            // Arrange
            var walletId = 1;
            var userId = "user1";
            var ownerId = "owner2"; // Not the actual owner
            var appUserWallet = new AppUser { Id = userId };
            var wallet = new UserWallet
            {
                Id = walletId,
                OwnerId = "owner1",
                AppUserWallets = new List<AppUser> { appUserWallet }
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            // Act
            await _walletService.RemoveMemberFromJointWalletAsync(walletId, userId, ownerId);

            // Assert: Exception is expected
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RemoveMemberFromJointWalletAsync_Should_Throw_When_User_Is_Not_Member()
        {
            // Arrange
            var walletId = 1;
            var userId = "user2"; // Not a member
            var ownerId = "owner1";
            var wallet = new UserWallet
            {
                Id = walletId,
                OwnerId = ownerId,
                AppUserWallets = new List<AppUser>()
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            // Act
            await _walletService.RemoveMemberFromJointWalletAsync(walletId, userId, ownerId);

            // Assert: Exception is expected
        }

        [TestMethod]
        public async Task ToggleOverdraftAsync_Should_Toggle_Overdraft_When_Conditions_Are_Met()
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ToggleOverdraftAsync_Should_Throw_When_Wallet_Not_Found()
        {
            // Arrange
            var walletId = 1;
            var userId = "user1";

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync((UserWallet)null);

            // Act
            await _walletService.ToggleOverdraftAsync(walletId, userId);

            // Assert: Exception is expected
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ToggleOverdraftAsync_Should_Throw_When_Wallet_Is_Not_Personal_Or_User_Is_Not_Owner()
        {
            // Arrange
            var walletId = 1;
            var userId = "user2"; // Not the owner
            var wallet = new UserWallet
            {
                Id = walletId,
                OwnerId = "owner1", // Different owner
                WalletType = WalletType.Joint // Not a personal wallet
            };

            _mockWalletRepository.Setup(repo => repo.GetWalletAsync(walletId))
                .ReturnsAsync(wallet);

            // Act
            await _walletService.ToggleOverdraftAsync(walletId, userId);

            // Assert: Exception is expected
        }

        [TestMethod]
        public async Task UpdateWalletAsync_Should_Call_Update_On_Repository()
        {
            // Act
            await _walletService.UpdateWalletAsync();

            // Assert
            _mockWalletRepository.Verify(repo => repo.UpdateWalletAsync(), Times.Once);
        }
    }
}
