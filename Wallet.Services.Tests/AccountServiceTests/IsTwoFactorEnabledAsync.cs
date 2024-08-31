using Microsoft.AspNetCore.Identity;
using Moq;
using Wallet.Data.Models;
using Wallet.Services.Implementations;

[TestClass]
public class IsTwoFactorEnabledAsyncTests
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
    public async Task ReturnTrue_When_TwoFactorIsEnabled()
    {
        // Arrange
        var user = new AppUser();
        _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user)).ReturnsAsync(true);

        // Act
        var result = await _sut.IsTwoFactorEnabledAsync(user);

        // Assert
        Assert.IsTrue(result);
        _mockUserManager.Verify(x => x.GetTwoFactorEnabledAsync(user), Times.Once);
    }

    [TestMethod]
    public async Task ReturnFalse_When_TwoFactorIsDisabled()
    {
        // Arrange
        var user = new AppUser();
        _mockUserManager.Setup(x => x.GetTwoFactorEnabledAsync(user)).ReturnsAsync(false);

        // Act
        var result = await _sut.IsTwoFactorEnabledAsync(user);

        // Assert
        Assert.IsFalse(result);
        _mockUserManager.Verify(x => x.GetTwoFactorEnabledAsync(user), Times.Once);
    }
}
