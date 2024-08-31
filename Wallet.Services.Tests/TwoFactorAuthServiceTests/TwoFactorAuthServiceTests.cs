using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
namespace Wallet.Services.Tests.TwoFactorAuthServiceTests
{
    [TestClass]
    public class TwoFactorAuthServiceTests
    {
        private Mock<IAccountService> _mockAccountService;
        private Mock<IEmailSender> _mockEmailSender;
        private TwoFactorAuthService _twoFactorAuthService;

        [TestInitialize]
        public void Setup()
        {
            _mockAccountService = new Mock<IAccountService>();
            _mockEmailSender = new Mock<IEmailSender>();
            _twoFactorAuthService = new TwoFactorAuthService(_mockAccountService.Object, _mockEmailSender.Object);
        }

        [TestMethod]
        public async Task GenerateQrCodeUriAsync_Should_Return_QrCodeUri()
        {
            // Arrange
            var user = new AppUser { Email = "test@example.com" };
            var key = "testkey";
            _mockAccountService.Setup(a => a.GetOrGenerateAuthenticatorKeyAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(key);

            // Act
            var result = await _twoFactorAuthService.GenerateQrCodeUriAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("otpauth://totp/"));
            Assert.IsTrue(result.Contains(UrlEncoder.Default.Encode(key)));
            Assert.IsTrue(result.Contains(UrlEncoder.Default.Encode(user.Email)));
        }

        [TestMethod]
        public async Task GenerateQrCodeImageAsync_Should_Return_ByteArray()
        {
            // Arrange
            var user = new AppUser { Email = "test@example.com" };
            var key = "testkey";
            _mockAccountService.Setup(a => a.GetOrGenerateAuthenticatorKeyAsync(It.IsAny<AppUser>()))
                .ReturnsAsync(key);

            // Act
            var result = await _twoFactorAuthService.GenerateQrCodeImageAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(byte[]));
        }

        [TestMethod]
        public async Task VerifyTwoFactorCodeAsync_Should_Return_True_When_Code_Is_Valid()
        {
            // Arrange
            var user = new AppUser();
            var code = "123456";
            _mockAccountService.Setup(a => a.VerifyTwoFactorTokenAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, code);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task VerifyTwoFactorCodeAsync_Should_Return_False_When_Code_Is_Invalid()
        {
            // Arrange
            var user = new AppUser();
            var code = "123456";
            _mockAccountService.Setup(a => a.VerifyTwoFactorTokenAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, code);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task EnableTwoFactorAuthenticationAsync_Should_Enable_TwoFactorAuthentication()
        {
            // Arrange
            var user = new AppUser();

            // Act
            await _twoFactorAuthService.EnableTwoFactorAuthenticationAsync(user);

            // Assert
            _mockAccountService.Verify(a => a.SetTwoFactorEnabledAsync(user, true), Times.Once);
        }

        [TestMethod]
        public async Task DisableTwoFactorAuthenticationAsync_Should_Disable_TwoFactorAuthentication()
        {
            // Arrange
            var user = new AppUser();

            // Act
            await _twoFactorAuthService.DisableTwoFactorAuthenticationAsync(user);

            // Assert
            _mockAccountService.Verify(a => a.SetTwoFactorEnabledAsync(user, false), Times.Once);
        }
    }
}
