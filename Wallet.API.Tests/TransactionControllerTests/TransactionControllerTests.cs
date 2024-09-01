using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Wallet.API.Controllers;
using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory;

namespace Wallet.API.Tests.TransactionControllerTests
{
    [TestClass]
    public class TransactionsControllerTests
    {
        private Mock<ITransactionService> _mockTransactionService;
        private TransactionsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockTransactionService = new Mock<ITransactionService>();

            _controller = new TransactionsController(_mockTransactionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.UserData, "testUserId")
                        }, "mock"))
                    }
                }
            };
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_Ok_When_Successful()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel { Amount = 100 };

            _mockTransactionService
                .Setup(service => service.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), "testUserId", null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateTransaction(transactionRequest);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
           // Assert.AreEqual(Messages.Controller.TransactionCreatedSuccessfully, ((dynamic)okResult.Value).message);
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_VerificationRequired_When_Throws_VerificationRequiredException()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel { Amount = 100 };
            _mockTransactionService
                .Setup(service => service.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), "testUserId", null))
                .ThrowsAsync(new VerificationRequiredException("token", 1, 100, "description", 2));

            // Act
            var result = await _controller.CreateTransaction(transactionRequest);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

            var verificationResponse = okResult.Value as VerificationRequiredResponse;
            Assert.IsNotNull(verificationResponse);
            Assert.AreEqual("token", verificationResponse.TransactionToken);
            Assert.AreEqual(Messages.Controller.VerificationRequired, verificationResponse.Message);
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_BadRequest_When_Throws_ArgumentException()
        {
            // Arrange
            var transactionRequest = new TransactionRequestModel { Amount = 100 };
            _mockTransactionService
                .Setup(service => service.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), "testUserId", null))
                .ThrowsAsync(new ArgumentException("Invalid transaction"));

            // Act
            var result = await _controller.CreateTransaction(transactionRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            //Assert.AreEqual("Invalid transaction", ((dynamic)badRequestResult.Value).error);
        }

        [TestMethod]
        public async Task GetTransactions_Should_Return_Ok_With_Transactions()
        {
            // Arrange
            var filter = new TransactionRequestFilter();
            var transactions = new List<TransactionDto>
            {
                new TransactionDto { Amount = 100, Description = "Test Transaction 1" },
                new TransactionDto { Amount = 200, Description = "Test Transaction 2" }
            };

            _mockTransactionService
                .Setup(service => service.FilterTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), filter, "testUserId"))
                .ReturnsAsync(transactions);

            // Act
            var result = await _controller.GetTransactions(filter);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(transactions, okResult.Value);
        }

        [TestMethod]
        public async Task VerifyTransaction_Should_Return_Ok_When_Verification_Succeeds()
        {
            // Arrange
            var verifyRequest = new VerifyTransactionRequestModel { Token = "token", VerificationCode = "123456" };

            _mockTransactionService
                .Setup(service => service.VerifyTransactionAsync(verifyRequest.Token, verifyRequest.VerificationCode))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.VerifyTransaction(verifyRequest);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            //Assert.AreEqual(Messages.Controller.TransactionVerifiedSuccessfully, ((dynamic)okResult.Value).message);
        }

        [TestMethod]
        public async Task VerifyTransaction_Should_Return_BadRequest_When_Verification_Fails()
        {
            // Arrange
            var verifyRequest = new VerifyTransactionRequestModel { Token = "token", VerificationCode = "123456" };

            _mockTransactionService
                .Setup(service => service.VerifyTransactionAsync(verifyRequest.Token, verifyRequest.VerificationCode))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.VerifyTransaction(verifyRequest);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            //Assert.AreEqual(Messages.Controller.VerificationFailed, ((dynamic)badRequestResult.Value).error);
        }

        [TestMethod]
        public async Task CancelRecurringTransaction_Should_Return_Ok_When_Successful()
        {
            // Arrange
            var transactionId = 1;

            _mockTransactionService
                .Setup(service => service.CancelRecurringTransactionAsync(transactionId, "testUserId"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CancelRecurringTransaction(transactionId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.AreEqual(Messages.Controller.RecurringTransactionCancelledSuccessfully, okResult.Value);
        }

        [TestMethod]
        public async Task AddTransactionToCategory_Should_Return_Ok_When_Successful()
        {
            // Arrange
            var categoryId = 1;
            var transactionId = 1;

            _mockTransactionService
                .Setup(service => service.AddTransactionToCategoryAsync(transactionId, categoryId, "testUserId"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddTransactionToCategory(categoryId, transactionId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
           // Assert.AreEqual(Messages.Controller.TransactionAddedToCategorySuccessfully, ((dynamic)okResult.Value).message);
        }

        [TestMethod]
        public async Task GetTransactions_Should_Return_BadRequest_When_PageOrPageSize_Invalid()
        {
            // Arrange
            var filter = new TransactionRequestFilter();

            // Act
            var result = await _controller.GetTransactions(filter, 0, 10);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task GetTransactions_Should_Return_Unauthorized_When_UserId_Is_Missing()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _controller.GetTransactions(new TransactionRequestFilter());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetTransactions_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.FilterTransactionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TransactionRequestFilter>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.GetTransactions(new TransactionRequestFilter());

            // Assert
            var internalServerErrorResult = result as ObjectResult;
            Assert.IsNotNull(internalServerErrorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
            Assert.AreEqual("Test Exception", internalServerErrorResult.Value.GetType().GetProperty("details").GetValue(internalServerErrorResult.Value));
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_BadRequest_When_ModelState_Is_Invalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.CreateTransaction(new TransactionRequestModel());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_Unauthorized_When_UserId_Is_Missing()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _controller.CreateTransaction(new TransactionRequestModel());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_VerificationRequired_When_VerificationRequiredException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), It.IsAny<string>(), null))
                .ThrowsAsync(new VerificationRequiredException("token", 1, 100, "description", 2));

            // Act
            var result = await _controller.CreateTransaction(new TransactionRequestModel());

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            var verificationResponse = okResult.Value as VerificationRequiredResponse;
            Assert.IsNotNull(verificationResponse);
            Assert.AreEqual("token", verificationResponse.TransactionToken);
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_BadRequest_When_ArgumentException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), It.IsAny<string>(), null))
                .ThrowsAsync(new ArgumentException("Invalid argument"));

            // Act
            var result = await _controller.CreateTransaction(new TransactionRequestModel());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid argument", badRequestResult.Value.GetType().GetProperty("error").GetValue(badRequestResult.Value));
        }

        [TestMethod]
        public async Task CreateTransaction_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.CreateTransactionAsync(It.IsAny<TransactionRequestModel>(), It.IsAny<string>(), null))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.CreateTransaction(new TransactionRequestModel());

            // Assert
            var internalServerErrorResult = result as ObjectResult;
            Assert.IsNotNull(internalServerErrorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
            Assert.AreEqual("Test Exception", internalServerErrorResult.Value.GetType().GetProperty("details").GetValue(internalServerErrorResult.Value));
        }

        [TestMethod]
        public async Task VerifyTransaction_Should_Return_BadRequest_When_ModelState_Is_Invalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.VerifyTransaction(new VerifyTransactionRequestModel());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task VerifyTransaction_Should_Return_Unauthorized_When_UserId_Is_Missing()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _controller.VerifyTransaction(new VerifyTransactionRequestModel());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task VerifyTransaction_Should_Return_BadRequest_When_ArgumentException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.VerifyTransactionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid argument"));

            // Act
            var result = await _controller.VerifyTransaction(new VerifyTransactionRequestModel());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid argument", badRequestResult.Value.GetType().GetProperty("error").GetValue(badRequestResult.Value));
        }

        [TestMethod]
        public async Task VerifyTransaction_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.VerifyTransactionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.VerifyTransaction(new VerifyTransactionRequestModel());

            // Assert
            var internalServerErrorResult = result as ObjectResult;
            Assert.IsNotNull(internalServerErrorResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, internalServerErrorResult.StatusCode);
            Assert.AreEqual("Test Exception", internalServerErrorResult.Value.GetType().GetProperty("details").GetValue(internalServerErrorResult.Value));
        }

        [TestMethod]
        public async Task CancelRecurringTransaction_Should_Return_Forbid_When_UnauthorizedAccessException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.CancelRecurringTransactionAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

            // Act
            var result = await _controller.CancelRecurringTransaction(1);

            // Assert
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }

        [TestMethod]
        public async Task CancelRecurringTransaction_Should_Return_NotFound_When_ArgumentException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.CancelRecurringTransactionAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Not found"));

            // Act
            var result = await _controller.CancelRecurringTransaction(1);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            Assert.AreEqual("Not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task CancelRecurringTransaction_Should_Return_BadRequest_When_InvalidOperationException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.CancelRecurringTransactionAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.CancelRecurringTransaction(1);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.AreEqual("Invalid operation", badRequestResult.Value);
        }


        [TestMethod]
        public async Task AddTransactionToCategory_Should_Return_NotFound_When_EntityNotFoundException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.AddTransactionToCategoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new EntityNotFoundException("Entity not found"));

            // Act
            var result = await _controller.AddTransactionToCategory(1, 1);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            Assert.AreEqual("Entity not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task AddTransactionToCategory_Should_Return_Forbid_When_UnauthorizedAccessException_Thrown()
        {
            // Arrange
            _mockTransactionService.Setup(service => service.AddTransactionToCategoryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

            // Act
            var result = await _controller.AddTransactionToCategory(1, 1);

            // Assert
            var forbidResult = result as ForbidResult;
            Assert.IsNotNull(forbidResult);
        }


    }
}
