using Digital_Wallet.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory;

namespace Wallet.API.Tests.AdminControllerTests
{
    [TestClass]
    public class AdminControllerTests
    {

        private Mock<IUserService> _mockUserService;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private Mock<IOverdraftSettingsService> _mockOverdraftSettingsService;
        private AdminController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockOverdraftSettingsService = new Mock<IOverdraftSettingsService>();
            var mockUserStore = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(mockUserStore.Object, null, null, null, null, null, null, null, null);

            _controller = new AdminController(_mockUserService.Object, _mockUserManager.Object, _mockOverdraftSettingsService.Object);
        }

        [TestMethod]
        public async Task GetUsers_ReturnsOkResult_WithUserList()
        {
            // Arrange
            var pagedResult = new PagedResult<UserWithRolesDto>
            {
                Items = new List<UserWithRolesDto> // Corrected from 'Results' to 'Items'
                {
                    new UserWithRolesDto { User = new AppUser { Id = "1", UserName = "testuser1" }, Roles = new List<string> { "Admin" } },
                    new UserWithRolesDto { User = new AppUser { Id = "2", UserName = "testuser2" }, Roles = new List<string> { "User" } }
                },
                TotalCount = 2
            };

            _mockUserService.Setup(service => service.SearchUsersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(pagedResult); // Correct type used in ReturnsAsync

            // Act
            var result = await _controller.GetUsers(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(pagedResult, okResult.Value);
        }

        [TestMethod]
        public async Task GetUserById_ReturnsOkResult_WithUser()
        {
            // Arrange
            var user = new AppUser { Id = "1", UserName = "testuser1" };
            _mockUserService.Setup(service => service.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById("1");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(user, okResult.Value);
        }

        [TestMethod]
        public async Task GetUserById_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            _mockUserService.Setup(service => service.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.GetUserById("1");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(Messages.UserNotFound, notFoundResult.Value);
        }

        [TestMethod]
        public async Task ManageRole_ReturnsOkResult_WhenActionSuccessful()
        {
            // Arrange
            var identityResult = IdentityResult.Success;
            _mockUserService.Setup(service => service.ManageRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _controller.ManageRole("userId", "add");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(string.Format(Messages.Controller.ActionSuccessful, "add"), okResult.Value);
        }

        [TestMethod]
        public async Task ManageRole_ReturnsBadRequest_WhenActionFails()
        {
            // Arrange
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Failed to add role." });
            _mockUserService.Setup(service => service.ManageRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _controller.ManageRole("userId", "add");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual($"Failed to perform 'add' action on the user. Errors: Failed to add role.", badRequestResult.Value);
        }


        [TestMethod]
        public async Task ManageRole_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
        {
            // Arrange
            _mockUserService.Setup(service => service.ManageRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            // Act
            var result = await _controller.ManageRole("invalidUserId", "add");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("User not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task ManageRole_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            _mockUserService.Setup(service => service.ManageRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid action"));

            // Act
            var result = await _controller.ManageRole("userId", "invalidAction");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Invalid action", badRequestResult.Value);
        }

        [TestMethod]
        public async Task ManageRole_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockUserService.Setup(service => service.ManageRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.ManageRole("userId", "add");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual(Messages.OperationFailed, objectResult.Value);
        }

        [TestMethod]
        public async Task SetInterestRate_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            _mockOverdraftSettingsService.Setup(service => service.SetInterestRateAsync(It.IsAny<decimal>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SetInterestRate(0.05m);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(string.Format(Messages.Controller.InterestRateSuccessful, 0.05m), okResult.Value);
        }

        [TestMethod]
        public async Task SetInterestRate_ReturnsBadRequest_WhenFailed()
        {
            // Arrange
            _mockOverdraftSettingsService.Setup(service => service.SetInterestRateAsync(It.IsAny<decimal>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SetInterestRate(0.05m);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task SetOverdraftLimit_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            _mockOverdraftSettingsService.Setup(service => service.SetOverdraftLimitAsync(It.IsAny<decimal>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SetOverdraftLimit(1000);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(string.Format(Messages.Controller.OverdraftLimitSuccessful, 1000), okResult.Value);
        }

        [TestMethod]
        public async Task SetOverdraftLimit_ReturnsBadRequest_WhenFailed()
        {
            // Arrange
            _mockOverdraftSettingsService.Setup(service => service.SetOverdraftLimitAsync(It.IsAny<decimal>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SetOverdraftLimit(1000);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task SetConsecutiveNegativeMonths_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            _mockOverdraftSettingsService.Setup(service => service.SetConsecutiveNegativeMonthsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SetConsecutiveNegativeMonths(3);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(string.Format(Messages.Controller.NegativeMonthsSuccessful, 3), okResult.Value);
        }

        [TestMethod]
        public async Task SetConsecutiveNegativeMonths_ReturnsBadRequest_WhenFailed()
        {
            // Arrange
            _mockOverdraftSettingsService.Setup(service => service.SetConsecutiveNegativeMonthsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SetConsecutiveNegativeMonths(3);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(Messages.Controller.NegativeMonthsFailed, badRequestResult.Value);
        }

    }
}
