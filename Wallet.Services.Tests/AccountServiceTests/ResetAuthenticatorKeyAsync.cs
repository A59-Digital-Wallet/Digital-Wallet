﻿using Microsoft.AspNetCore.Identity;
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
    public class ResetAuthenticatorKeyAsyncTests
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
        public async Task ResetKey_When_Called()
        {
            // Arrange
            var user = new AppUser();
            _mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _sut.ResetAuthenticatorKeyAsync(user);

            // Assert
            _mockUserManager.Verify(x => x.ResetAuthenticatorKeyAsync(user), Times.Once);
        }
    }

}
