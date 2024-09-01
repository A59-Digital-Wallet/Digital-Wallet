using Digital_Wallet.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Wallet.API.Tests.ContactControllerTests
{
    [TestClass]
    public class ContactControllerTests
    {
        private Mock<IContactService> _mockContactService;
        private ContactController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockContactService = new Mock<IContactService>();

            _controller = new ContactController(_mockContactService.Object);

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
        public async Task GetContactsAsync_ReturnsOkResult_WithContacts()
        {
            // Arrange
            var userId = "test-user-id";
            var contacts = new List<ContactResponseDTO>
            {
                new ContactResponseDTO { Id = "1", FirstName = "John", LastName = "Doe" },
                new ContactResponseDTO { Id = "2", FirstName = "Jane", LastName = "Smith" }
            };

            _mockContactService.Setup(service => service.GetContactsAsync(userId)).ReturnsAsync(contacts);

            // Act
            var result = await _controller.GetContactsAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            CollectionAssert.AreEqual(contacts, (List<ContactResponseDTO>)okResult.Value);
        }

        [TestMethod]
        public async Task GetContactsAsync_ReturnsBadRequest_WhenEntityNotFoundExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            _mockContactService.Setup(service => service.GetContactsAsync(userId))
                .ThrowsAsync(new EntityNotFoundException("Contacts not found"));

            // Act
            var result = await _controller.GetContactsAsync();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Contacts not found", badRequestResult.Value);
        }

        [TestMethod]
        public async Task AddContact_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var userId = "test-user-id";
            var contactId = "contact-id";

            _mockContactService.Setup(service => service.AddContactAsync(userId, contactId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddContact(contactId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var messageProperty = okResult.Value.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty);
            Assert.AreEqual(Messages.Controller.ContactAddedSuccessful, messageProperty.GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task AddContact_ReturnsNotFound_WhenEntityNotFoundExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var contactId = "contact-id";

            _mockContactService.Setup(service => service.AddContactAsync(userId, contactId))
                .ThrowsAsync(new EntityNotFoundException("Contact not found"));

            // Act
            var result = await _controller.AddContact(contactId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Contact not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task AddContact_ReturnsBadRequest_WhenInvalidOperationExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var contactId = "contact-id";

            _mockContactService.Setup(service => service.AddContactAsync(userId, contactId))
                .ThrowsAsync(new InvalidOperationException("Contact already exists"));

            // Act
            var result = await _controller.AddContact(contactId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual("Contact already exists", badRequestResult.Value);
        }

        [TestMethod]
        public async Task RemoveContatctAsync_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var userId = "test-user-id";
            var contactId = "contact-id";

            _mockContactService.Setup(service => service.RemoveContactAsync(userId, contactId)).ReturnsAsync(true);

            // Act
            var result = await _controller.RemoveContatctAsync(contactId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var messageProperty = okResult.Value.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty);
            Assert.AreEqual(Messages.Controller.ContactDeletedSuccessful, messageProperty.GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task RemoveContatctAsync_ReturnsNotFound_WhenEntityNotFoundExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var contactId = "contact-id";

            _mockContactService.Setup(service => service.RemoveContactAsync(userId, contactId))
                .ThrowsAsync(new EntityNotFoundException("Contact not found"));

            // Act
            var result = await _controller.RemoveContatctAsync(contactId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Contact not found", notFoundResult.Value);
        }
    }
}
