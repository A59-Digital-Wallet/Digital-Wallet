using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
using Wallet.DTO.Request;

namespace Wallet.Services.Tests.SavingsInterestServiceTests
{
    [TestClass]
    public class SavingsInterestServiceTests
    {
        private Mock<IWalletRepository> _mockWalletRepository;
        private Mock<ITransactionService> _mockTransactionService;
        private SavingsInterestService _savingsInterestService;

        [TestInitialize]
        public void Setup()
        {
            _mockWalletRepository = new Mock<IWalletRepository>();
            _mockTransactionService = new Mock<ITransactionService>();
            _savingsInterestService = new SavingsInterestService(_mockWalletRepository.Object, _mockTransactionService.Object);
        }

        [TestMethod]
        public async Task ApplyMonthlyInterestAsync_Should_Apply_Interest_And_Create_Transaction()
        {
            // Arrange
            var wallet = new UserWallet
            {
                Id = 1,
                Balance = 1000m,
                WalletType = WalletType.Savings,
                OwnerId = "owner-1"
            };
            var savingsWallets = new List<UserWallet> { wallet };

            _mockWalletRepository.Setup(repo => repo.GetSavingsWalletsAsync())
                .ReturnsAsync(savingsWallets);

            _mockWalletRepository.Setup(repo => repo.UpdateWalletAsync())
                .ReturnsAsync(true);

            // Act
            await _savingsInterestService.ApplyMonthlyInterestAsync();

            // Assert
            var expectedInterestAmount = 1000 * (0.046m / 12);
            var expectedBalance = 1000 + expectedInterestAmount;

            Assert.AreEqual(expectedBalance, wallet.Balance);

            _mockWalletRepository.Verify(repo => repo.UpdateWalletAsync(), Times.Once);

          
        }
    }
}
