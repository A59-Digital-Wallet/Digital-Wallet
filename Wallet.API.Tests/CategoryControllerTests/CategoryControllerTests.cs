using Digital_Wallet.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Wallet.API.Tests.CategoryControllerTests
{
    [TestClass]
    public class CategoryControllerTests
    {
        private Mock<ICategoryService> _mockCategoryService;
        private CategoryController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCategoryService = new Mock<ICategoryService>();

            _controller = new CategoryController(_mockCategoryService.Object);

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
        public async Task GetUserCategories_ReturnsOkResult_WithCategories()
        {
            // Arrange
            var userId = "test-user-id";
            var categories = new List<CategoryResponseDTO>
            {
                new CategoryResponseDTO { Id = 1, Name = "Category 1" },
                new CategoryResponseDTO { Id = 2, Name = "Category 2" }
            };

            _mockCategoryService.Setup(service => service.GetUserCategoriesAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.GetUserCategories();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            CollectionAssert.AreEqual(categories, (List<CategoryResponseDTO>)okResult.Value);
        }

        [TestMethod]
        public async Task GetUserCategories_ReturnsNotFound_WhenEntityNotFoundExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            _mockCategoryService.Setup(service => service.GetUserCategoriesAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new EntityNotFoundException("Categories not found"));

            // Act
            var result = await _controller.GetUserCategories();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Categories not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task AddCategory_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryRequest = new CategoryRequestDTO { Name = "New Category" };

            _mockCategoryService.Setup(service => service.AddCategoryAsync(userId, categoryRequest))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddCategory(categoryRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var messageProperty = okResult.Value.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty);
            Assert.AreEqual(Messages.Controller.CategoryAddedSuccessful, messageProperty.GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task AddCategory_ReturnsBadRequest_WhenArgumentNullExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryRequest = new CategoryRequestDTO { Name = null }; // Invalid input

            _mockCategoryService.Setup(service => service.AddCategoryAsync(userId, categoryRequest))
                .ThrowsAsync(new ArgumentNullException(nameof(categoryRequest), Messages.Service.CategoryNameCannotBeEmpty));

            // Act
            var result = await _controller.AddCategory(categoryRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var actualMessage = ExtractActualMessageFromException(badRequestResult.Value.ToString());

            Assert.AreEqual(Messages.Service.CategoryNameCannotBeEmpty, actualMessage);
        }

        [TestMethod]
        public async Task UpdateCategory_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryId = 1;
            var categoryRequest = new CategoryRequestDTO { Name = "Updated Category" };
            var updatedCategory = new CategoryResponseDTO { Id = 1, Name = "Updated Category" };

            _mockCategoryService.Setup(service => service.UpdateCategoryAsync(userId, categoryId, categoryRequest))
                .ReturnsAsync(updatedCategory);

            // Act
            var result = await _controller.UpdateCategory(categoryId, categoryRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(updatedCategory, okResult.Value);
        }

        [TestMethod]
        public async Task UpdateCategory_ReturnsBadRequest_WhenArgumentNullExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryId = 1;
            var categoryRequest = new CategoryRequestDTO { Name = null }; // Invalid input

            _mockCategoryService.Setup(service => service.UpdateCategoryAsync(userId, categoryId, categoryRequest))
                .ThrowsAsync(new ArgumentNullException(nameof(categoryRequest), Messages.Service.CategoryNameCannotBeEmpty)); // Use custom message

            // Act
            var result = await _controller.UpdateCategory(categoryId, categoryRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;

            var actualMessage = ExtractActualMessageFromException(badRequestResult.Value.ToString());
            Assert.AreEqual(Messages.Service.CategoryNameCannotBeEmpty, actualMessage);
        }



        [TestMethod]
        public async Task UpdateCategory_ReturnsNotFound_WhenEntityNotFoundExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryId = 1;
            var categoryRequest = new CategoryRequestDTO { Name = "Updated Category" };

            _mockCategoryService.Setup(service => service.UpdateCategoryAsync(userId, categoryId, categoryRequest))
                .ThrowsAsync(new EntityNotFoundException("Category not found"));

            // Act
            var result = await _controller.UpdateCategory(categoryId, categoryRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Category not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task UpdateCategory_ReturnsForbid_WhenAuthorizationExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryId = 1;
            var categoryRequest = new CategoryRequestDTO { Name = "Updated Category" };

            _mockCategoryService.Setup(service => service.UpdateCategoryAsync(userId, categoryId, categoryRequest))
                .ThrowsAsync(new AuthorizationException("Not authorized"));

            // Act
            var result = await _controller.UpdateCategory(categoryId, categoryRequest);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task DeleteCategoryAsync_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryId = 1;

            // Corrected mock setup
            _mockCategoryService.Setup(service => service.DeleteCategoryAsync(userId, categoryId)).Returns(Task.CompletedTask);  // Ensure ReturnsAsync is used correctly with the expected return type

            // Act
            var result = await _controller.DeleteCategoryAsync(categoryId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            // Correctly access the 'message' property from the anonymous object
            var messageProperty = okResult.Value.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty);
            Assert.AreEqual(Messages.Controller.CategoryDeletedSuccessful, messageProperty.GetValue(okResult.Value));
        }

        [TestMethod]
        public async Task DeleteCategoryAsync_ReturnsNotFound_WhenEntityNotFoundExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryId = 1;

            _mockCategoryService.Setup(service => service.DeleteCategoryAsync(userId, categoryId))
                .ThrowsAsync(new EntityNotFoundException("Category not found"));

            // Act
            var result = await _controller.DeleteCategoryAsync(categoryId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Category not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task DeleteCategoryAsync_ReturnsForbid_WhenAuthorizationExceptionThrown()
        {
            // Arrange
            var userId = "test-user-id";
            var categoryId = 1;

            _mockCategoryService.Setup(service => service.DeleteCategoryAsync(userId, categoryId))
                .ThrowsAsync(new AuthorizationException("Not authorized"));

            // Act
            var result = await _controller.DeleteCategoryAsync(categoryId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        private string ExtractActualMessageFromException(string exceptionMessage)
        {
            // Extracts the actual message without the parameter part
            var index = exceptionMessage.IndexOf(" (Parameter");
            if (index > 0)
            {
                return exceptionMessage.Substring(0, index);
            }
            return exceptionMessage;
        }
    }
}
