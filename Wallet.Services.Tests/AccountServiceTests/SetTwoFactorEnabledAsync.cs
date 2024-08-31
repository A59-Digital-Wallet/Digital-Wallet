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
    public class SetTwoFactorEnabledAsyncTests
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
        public async Task SetTwoFactorEnabled_When_Called()
        {
            // Arrange
            var user = new AppUser();
            var enabled = true;

            _mockUserManager.Setup(x => x.SetTwoFactorEnabledAsync(user, enabled)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _sut.SetTwoFactorEnabledAsync(user, enabled);

            // Assert
            _mockUserManager.Verify(x => x.SetTwoFactorEnabledAsync(user, enabled), Times.Once);
        }
    }

}
