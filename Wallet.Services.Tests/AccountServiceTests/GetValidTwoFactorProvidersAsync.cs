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
    public class GetValidTwoFactorProvidersAsyncTests
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
        public async Task ReturnListOfProviders_When_ProvidersExist()
        {
            // Arrange
            var user = new AppUser();
            var providers = new List<string> { "Authenticator", "Email" };
            _mockUserManager.Setup(x => x.GetValidTwoFactorProvidersAsync(user)).ReturnsAsync(providers);

            // Act
            var result = await _sut.GetValidTwoFactorProvidersAsync(user);

            // Assert
            Assert.AreEqual(providers, result);
            _mockUserManager.Verify(x => x.GetValidTwoFactorProvidersAsync(user), Times.Once);
        }
    }
}
