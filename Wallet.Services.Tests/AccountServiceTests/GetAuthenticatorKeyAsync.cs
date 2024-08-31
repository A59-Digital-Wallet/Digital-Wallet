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
    public class GetAuthenticatorKeyAsyncTests
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
        public async Task ReturnKey_When_KeyExists()
        {
            // Arrange
            var user = new AppUser();
            var expectedKey = "authenticator-key";
            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user)).ReturnsAsync(expectedKey);

            // Act
            var result = await _sut.GetAuthenticatorKeyAsync(user);

            // Assert
            Assert.AreEqual(expectedKey, result);
            _mockUserManager.Verify(x => x.GetAuthenticatorKeyAsync(user), Times.Once);
        }
    }
}
