using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Implementations;

namespace Wallet.UnitTests.Services.AccountServiceTests
{
    [TestClass]
    public class GetOrGenerateAuthenticatorKeyAsyncTests
    {
        private Mock<UserManager<AppUser>> _mockUserManager;
        private AccountService _sut; // System Under Test

        [TestInitialize]
        public void Setup()
        {
            // Mocking the UserManager<AppUser>
            _mockUserManager = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

            // Initializing the AccountService with the mocked UserManager
            _sut = new AccountService(_mockUserManager.Object);
        }

        [TestMethod]
        public async Task ReturnExistingAuthenticatorKey_When_KeyExists()
        {
            // Arrange
            var user = new AppUser();
            var expectedKey = "existing-key";

            _mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user))
                            .ReturnsAsync(expectedKey);

            // Act
            var result = await _sut.GetOrGenerateAuthenticatorKeyAsync(user);

            // Assert
            Assert.AreEqual(expectedKey, result);
            _mockUserManager.Verify(x => x.GetAuthenticatorKeyAsync(user), Times.Once);
            _mockUserManager.Verify(x => x.ResetAuthenticatorKeyAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [TestMethod]
        public async Task GenerateNewAuthenticatorKey_When_KeyDoesNotExist()
        {
            // Arrange
            var user = new AppUser();
            var newKey = "new-key";

            _mockUserManager.SetupSequence(x => x.GetAuthenticatorKeyAsync(user))
                            .ReturnsAsync((string)null) // First call returns null
                            .ReturnsAsync(newKey); // Second call returns the new key

            _mockUserManager.Setup(x => x.ResetAuthenticatorKeyAsync(user))
                            .ReturnsAsync(IdentityResult.Success); // This is the corrected part

            // Act
            var result = await _sut.GetOrGenerateAuthenticatorKeyAsync(user);

            // Assert
            Assert.AreEqual(newKey, result);
            _mockUserManager.Verify(x => x.GetAuthenticatorKeyAsync(user), Times.Exactly(2));
            _mockUserManager.Verify(x => x.ResetAuthenticatorKeyAsync(user), Times.Once);
        }
    }
}
