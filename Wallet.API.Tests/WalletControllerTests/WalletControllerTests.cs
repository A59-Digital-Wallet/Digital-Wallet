using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using Wallet.API.Controllers;
using Wallet.Common.Helpers;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Wallet.API.Tests.WalletControllerTests
{
    [TestClass]
    public class WalletControllerTests
    {
        private Mock<IWalletService> _mockWalletService;
        private WalletController _controller;

        [TestInitialize]
        public void SetUp()
        {
            _mockWalletService = new Mock<IWalletService>();

            _controller = new WalletController(_mockWalletService.Object);

            // Mock the HttpContext and set up a ClaimsPrincipal
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.UserData, "user123"),
            }, "mock"));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.User).Returns(user);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };
        }

        [TestMethod]
        public async Task CreateWallet_Should_Return_Ok_When_Wallet_Created_Successfully()
        {
            // Arrange
            var userWalletRequest = new UserWalletRequest();
            var userId = "user123";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.CreateWallet(userWalletRequest, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateWallet(userWalletRequest);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(Messages.Controller.WalletCreatedSuccessfully, okResult.Value.GetType().GetProperty("message").GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task GetWallet_Should_Return_Ok_When_Wallet_Found()
        {
            // Arrange
            var walletId = 1;
            var userId = "user123";
            var walletResponse = new WalletResponseDTO();
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.GetWalletAsync(walletId, userId))
                .ReturnsAsync(walletResponse);

            // Act
            var result = await _controller.GetWallet(walletId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(walletResponse, okResult.Value);
        }

        [TestMethod]
        public async Task GetWallet_Should_Return_Forbid_When_UnauthorizedAccessException_Thrown()
        {
            // Arrange
            var walletId = 1;
            var userId = "user123";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.GetWalletAsync(walletId, userId))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.GetWallet(walletId);

            // Assert
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TestMethod]
        public async Task GetWallet_Should_Return_NotFound_When_ArgumentException_Thrown()
        {
            // Arrange
            var walletId = 1;
            var userId = "user123";
            var exceptionMessage = "Wallet not found";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.GetWalletAsync(walletId, userId))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.GetWallet(walletId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            Assert.AreEqual(exceptionMessage, notFoundResult.Value.GetType().GetProperty("message").GetValue(notFoundResult.Value));
        }

        [TestMethod]
        public async Task AddMemberToJointWallet_Should_Return_Ok_When_Member_Added_Successfully()
        {
            // Arrange
            var walletId = 1;
            var model = new ManagePermissionsModel { UserId = "member123", CanSpend = true, CanAddFunds = true };
            var userId = "user123";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.AddMemberToJointWalletAsync(walletId, model.UserId, model.CanSpend, model.CanAddFunds, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddMemberToJointWallet(walletId, model);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(Messages.Controller.MemberAddedToWalletSuccess, okResult.Value.GetType().GetProperty("message").GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task AddMemberToJointWallet_Should_Return_BadRequest_When_ArgumentException_Thrown()
        {
            // Arrange
            var walletId = 1;
            var model = new ManagePermissionsModel { UserId = "member123", CanSpend = true, CanAddFunds = true };
            var userId = "user123";
            var exceptionMessage = "Invalid wallet";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.AddMemberToJointWalletAsync(walletId, model.UserId, model.CanSpend, model.CanAddFunds, userId))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.AddMemberToJointWallet(walletId, model);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(exceptionMessage, badRequestResult.Value.GetType().GetProperty("message").GetValue(badRequestResult.Value));
        }
        [TestMethod]
        public async Task RemoveMemberFromJointWallet_Should_Return_Ok_When_Member_Removed_Successfully()
        {
            // Arrange
            var walletId = 1;
            var userIdToRemove = "member123";
            var userId = "user123";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.RemoveMemberFromJointWalletAsync(walletId, userIdToRemove, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RemoveMemberFromJointWallet(walletId, userIdToRemove);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(Messages.Controller.MemberRemovedFromWalletSuccess, okResult.Value.GetType().GetProperty("message").GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task RemoveMemberFromJointWallet_Should_Return_BadRequest_When_InvalidOperationException_Thrown()
        {
            // Arrange
            var walletId = 1;
            var userIdToRemove = "member123";
            var userId = "user123";
            var exceptionMessage = "User not found in wallet";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.RemoveMemberFromJointWalletAsync(walletId, userIdToRemove, userId))
                .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _controller.RemoveMemberFromJointWallet(walletId, userIdToRemove);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(exceptionMessage, badRequestResult.Value.GetType().GetProperty("message").GetValue(badRequestResult.Value));
        }
        [TestMethod]
        public async Task ToggleOverdraft_Should_Return_Ok_When_Overdraft_Toggled_Successfully()
        {
            // Arrange
            var walletId = 1;
            var userId = "user123";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.ToggleOverdraftAsync(walletId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ToggleOverdraft(walletId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(Messages.Controller.OverdraftUpdatedSuccessfully, okResult.Value.GetType().GetProperty("message").GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task ToggleOverdraft_Should_Return_BadRequest_When_Exception_Thrown()
        {
            // Arrange
            var walletId = 1;
            var userId = "user123";
            var exceptionMessage = "Operation failed";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.UserData, userId),
            }));

            _mockWalletService.Setup(service => service.ToggleOverdraftAsync(walletId, userId))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.ToggleOverdraft(walletId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual(exceptionMessage, badRequestResult.Value.GetType().GetProperty("message").GetValue(badRequestResult.Value));
        }



    }
}
