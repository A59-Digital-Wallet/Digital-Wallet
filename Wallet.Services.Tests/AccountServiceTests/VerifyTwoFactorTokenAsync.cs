using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.AccountServiceTests
{
    [TestClass]
    public class VerifyTwoFactorTokenAsyncTests
    {
        private Mock<UserManager<AppUser>> _mockUserManager;
        private AccountService _sut;

        [TestInitialize]
        public void Setup()
        {
            _mockUserManager = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

            _sut = new AccountService(_mockUserManager.Object);
        }

        [TestMethod]
        public async Task ReturnTrue_When_TokenIsValid()
        {
            // Arrange
            var user = new AppUser();
            var code = "123456";
            _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user, _mockUserManager.Object.Options.Tokens.AuthenticatorTokenProvider, code)).ReturnsAsync(true);

            // Act
            var result = await _sut.VerifyTwoFactorTokenAsync(user, code);

            // Assert
            Assert.IsTrue(result);
            _mockUserManager.Verify(x => x.VerifyTwoFactorTokenAsync(user, _mockUserManager.Object.Options.Tokens.AuthenticatorTokenProvider, code), Times.Once);
        }

        [TestMethod]
        public async Task ReturnFalse_When_TokenIsInvalid()
        {
            // Arrange
            var user = new AppUser();
            var code = "654321";
            _mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user, _mockUserManager.Object.Options.Tokens.AuthenticatorTokenProvider, code)).ReturnsAsync(false);

            // Act
            var result = await _sut.VerifyTwoFactorTokenAsync(user, code);

            // Assert
            Assert.IsFalse(result);
            _mockUserManager.Verify(x => x.VerifyTwoFactorTokenAsync(user, _mockUserManager.Object.Options.Tokens.AuthenticatorTokenProvider, code), Times.Once);
        }
    }

}
