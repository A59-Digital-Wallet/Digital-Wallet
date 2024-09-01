using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;  // Correct DTOs namespace
using Wallet.Services.Contracts;
using Digital_Wallet.Controllers;

namespace Wallet.API.Tests.CardControllerTests
{
    [TestClass]
    public class CardControllerTests
    {
        private Mock<ICardService> _mockCardService;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private CardController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCardService = new Mock<ICardService>();

            var mockUserStore = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(mockUserStore.Object, null, null, null, null, null, null, null, null);

            // Create the controller instance with mocked dependencies
            _controller = new CardController(_mockCardService.Object, _mockUserManager.Object);

            // Set up a mock user with a specific user ID
            var userId = "test-user-id";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.UserData, userId)
            };

            var mockIdentity = new ClaimsIdentity(claims);
            var mockPrincipal = new ClaimsPrincipal(mockIdentity);

            // Set up the controller context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockPrincipal }
            };
        }

        [TestMethod]
        public async Task GetCards_ReturnsOk_WithCards()
        {
            // Arrange
            var userId = "test-user-id";
            var cards = new List<CardResponseDTO>
            {
                new CardResponseDTO { Id = 1, CardHolderName = "John Doe", CardNumber = "1234567890123456" },
                new CardResponseDTO { Id = 2, CardHolderName = "Jane Doe", CardNumber = "9876543210987654" }
            }; // Adjust to use properties in CardResponseDTO
            _mockCardService.Setup(service => service.GetCardsAsync(userId)).ReturnsAsync(cards); // Corrected type

            // Act
            var result = await _controller.GetCards();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            CollectionAssert.AreEqual(cards, (List<CardResponseDTO>)okResult.Value);
        }

        [TestMethod]
        public async Task GetCards_ReturnsBadRequest_WhenEntityNotFoundExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            _mockCardService.Setup(service => service.GetCardsAsync(userId)).ThrowsAsync(new EntityNotFoundException("Cards not found"));

            // Act
            var result = await _controller.GetCards();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Cards not found", badRequestResult.Value);
        }

        [TestMethod]
        public async Task GetCard_ReturnsOk_WithCard()
        {
            // Arrange
            var userId = "test-user-id";
            var cardId = 1;
            var card = new CardResponseDTO { Id = 1, CardHolderName = "John Doe", CardNumber = "1234567890123456" }; // Adjusted to use properties in CardResponseDTO
            _mockCardService.Setup(service => service.GetCardAsync(cardId, userId)).ReturnsAsync(card); // Corrected type

            // Act
            var result = await _controller.GetCard(cardId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(card, okResult.Value);
        }

        [TestMethod]
        public async Task GetCard_ReturnsForbid_WhenAuthorizationExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var cardId = 1;
            _mockCardService.Setup(service => service.GetCardAsync(cardId, userId)).ThrowsAsync(new AuthorizationException("Not authorized"));

            // Act
            var result = await _controller.GetCard(cardId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task AddCard_ReturnsOk_WithSuccessMessage()
        {
            // Arrange
            var userId = "test-user-id";
            var cardRequest = new CardRequest();
            _mockCardService.Setup(service => service.AddCardAsync(cardRequest, userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddCard(cardRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);

            var messageProperty = resultValue.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty, "The result of object does not contain a 'message' property.");

            var messageValue = messageProperty.GetValue(resultValue)?.ToString();
            
            Assert.AreEqual(Messages.Controller.CardAddedSuccessful, messageValue);
        }

        [TestMethod]
        public async Task DeleteCard_ReturnsOk_WithSuccessMessage()
        {
            // Arrange
            var userId = "test-user-id";
            var cardId = 1;
            _mockCardService.Setup(service => service.DeleteCardAsync(cardId, userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCard(cardId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult)); 

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult); 

            var resultValue = okResult.Value;
            Assert.IsNotNull(resultValue);

            // Use reflection to access the 'message' property of the anonymous type
            var messageProperty = resultValue.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty, "The result object does not contain a 'message' property.");

            // Get the value of the 'message' property and assert it
            var messageValue = messageProperty.GetValue(resultValue)?.ToString();
            Assert.AreEqual(Messages.Controller.CardDeletedSuccessful, messageValue);
        }

        [TestMethod]
        public async Task DeleteCard_ReturnsForbid_WhenAuthorizationExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var cardId = 1;
            _mockCardService.Setup(service => service.DeleteCardAsync(cardId, userId)).ThrowsAsync(new AuthorizationException("Not authorized"));

            // Act
            var result = await _controller.DeleteCard(cardId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }
    }
}
