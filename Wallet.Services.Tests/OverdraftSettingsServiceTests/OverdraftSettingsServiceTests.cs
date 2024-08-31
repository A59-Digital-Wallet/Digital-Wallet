using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class OverdraftSettingsServiceTests
    {
        private Mock<IOverdraftSettingsRepository> _repositoryMock;
        private OverdraftSettingsService _service;

        [TestInitialize]
        public void SetUp()
        {
            _repositoryMock = new Mock<IOverdraftSettingsRepository>();
            _service = new OverdraftSettingsService(_repositoryMock.Object);
        }

        [TestMethod]
        public async Task GetSettingsAsync_ShouldReturnOverdraftSettings_WhenSettingsExist()
        {
            // Arrange
            var settings = new OverdraftSettings
            {
                DefaultInterestRate = 0.05m,
                DefaultOverdraftLimit = 1000m,
                DefaultConsecutiveNegativeMonths = 3
            };

            _repositoryMock.Setup(repo => repo.GetSettingsAsync()).ReturnsAsync(settings);

            // Act
            var result = await _service.GetSettingsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0.05m, result.DefaultInterestRate);
            Assert.AreEqual(1000m, result.DefaultOverdraftLimit);
            Assert.AreEqual(3, result.DefaultConsecutiveNegativeMonths);
        }

        [TestMethod]
        public async Task SetInterestRateAsync_ShouldUpdateInterestRate_WhenSettingsExist()
        {
            // Arrange
            var settings = new OverdraftSettings
            {
                DefaultInterestRate = 0.05m
            };

            _repositoryMock.Setup(repo => repo.GetSettingsAsync()).ReturnsAsync(settings);
            _repositoryMock.Setup(repo => repo.UpdateSettings(settings)).ReturnsAsync(true);

            var newRate = 7.5m; // 7.5%

            // Act
            var result = await _service.SetInterestRateAsync(newRate);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0.075m, settings.DefaultInterestRate); // Converted to 0.075 (7.5%)
            _repositoryMock.Verify(repo => repo.UpdateSettings(settings), Times.Once);
        }

        [TestMethod]
        public async Task SetInterestRateAsync_ShouldReturnFalse_WhenSettingsDoNotExist()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.GetSettingsAsync()).ReturnsAsync((OverdraftSettings)null);

            // Act
            var result = await _service.SetInterestRateAsync(5m);

            // Assert
            Assert.IsFalse(result);
            _repositoryMock.Verify(repo => repo.UpdateSettings(It.IsAny<OverdraftSettings>()), Times.Never);
        }

        [TestMethod]
        public async Task SetOverdraftLimitAsync_ShouldUpdateOverdraftLimit_WhenSettingsExist()
        {
            // Arrange
            var settings = new OverdraftSettings
            {
                DefaultOverdraftLimit = 1000m
            };

            _repositoryMock.Setup(repo => repo.GetSettingsAsync()).ReturnsAsync(settings);
            _repositoryMock.Setup(repo => repo.UpdateSettings(settings)).ReturnsAsync(true);

            var newLimit = 2000m;

            // Act
            var result = await _service.SetOverdraftLimitAsync(newLimit);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(2000m, settings.DefaultOverdraftLimit);
            _repositoryMock.Verify(repo => repo.UpdateSettings(settings), Times.Once);
        }

        [TestMethod]
        public async Task SetOverdraftLimitAsync_ShouldReturnFalse_WhenSettingsDoNotExist()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.GetSettingsAsync()).ReturnsAsync((OverdraftSettings)null);

            // Act
            var result = await _service.SetOverdraftLimitAsync(1500m);

            // Assert
            Assert.IsFalse(result);
            _repositoryMock.Verify(repo => repo.UpdateSettings(It.IsAny<OverdraftSettings>()), Times.Never);
        }

        [TestMethod]
        public async Task SetConsecutiveNegativeMonthsAsync_ShouldUpdateConsecutiveNegativeMonths_WhenSettingsExist()
        {
            // Arrange
            var settings = new OverdraftSettings
            {
                DefaultConsecutiveNegativeMonths = 3
            };

            _repositoryMock.Setup(repo => repo.GetSettingsAsync()).ReturnsAsync(settings);
            _repositoryMock.Setup(repo => repo.UpdateSettings(settings)).ReturnsAsync(true);

            var newMonths = 4;

            // Act
            var result = await _service.SetConsecutiveNegativeMonthsAsync(newMonths);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(4, settings.DefaultConsecutiveNegativeMonths);
            _repositoryMock.Verify(repo => repo.UpdateSettings(settings), Times.Once);
        }

        [TestMethod]
        public async Task SetConsecutiveNegativeMonthsAsync_ShouldReturnFalse_WhenSettingsDoNotExist()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.GetSettingsAsync()).ReturnsAsync((OverdraftSettings)null);

            // Act
            var result = await _service.SetConsecutiveNegativeMonthsAsync(2);

            // Assert
            Assert.IsFalse(result);
            _repositoryMock.Verify(repo => repo.UpdateSettings(It.IsAny<OverdraftSettings>()), Times.Never);
        }
    }
}
