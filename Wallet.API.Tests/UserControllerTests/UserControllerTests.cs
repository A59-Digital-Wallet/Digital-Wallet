using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Wallet.API.Controllers;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Common.Helpers;
using Wallet.Data.Helpers.Contracts;
using Digital_Wallet.Controllers.Digital_Wallet.Controllers;
using Microsoft.AspNetCore.Identity;
using Wallet.Data.Models;
using Wallet.Common.Exceptions;

namespace Wallet.API.Tests.UserControllerTests
{
    [TestClass]
    public class UserControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<IAuthManager> _mockAuthManager;
        private Mock<IAccountService> _mockAccountService;
        private Mock<ITwoFactorAuthService> _mockTwoFactorAuthService;
        private UserController _controller;

        [TestInitialize]
        public void SetUp()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAuthManager = new Mock<IAuthManager>();
            _mockAccountService = new Mock<IAccountService>();
            _mockTwoFactorAuthService = new Mock<ITwoFactorAuthService>();

            _controller = new UserController(_mockUserService.Object, _mockAuthManager.Object, _mockAccountService.Object, _mockTwoFactorAuthService.Object);

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
        public async Task Register_Should_Return_Ok_When_Registration_Is_Successful()
        {
            // Arrange
            var registerModel = new RegisterModel();
            _mockUserService.Setup(s => s.RegisterUserAsync(registerModel)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(Messages.Controller.RegistrationSuccess, okResult.Value);
        }
        [TestMethod]
        public async Task Register_Should_Return_BadRequest_When_Registration_Fails()
        {
            // Arrange
            var registerModel = new RegisterModel();
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Error" });
            _mockUserService.Setup(s => s.RegisterUserAsync(registerModel)).ReturnsAsync(identityResult);

            // Act
            var result = await _controller.Register(registerModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(identityResult.Errors, badRequestResult.Value);
        }

        [TestMethod]
        public async Task VerifyEmail_Should_Return_Ok_When_Verification_Is_Successful()
        {
            // Arrange
            var verifyEmailModel = new VerifyEmailModel();
            _mockUserService.Setup(s => s.VerifyEmailAsync(verifyEmailModel)).ReturnsAsync(true);

            // Act
            var result = await _controller.VerifyEmail(verifyEmailModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(Messages.Controller.EmailConfirmationSuccess, okResult.Value);
        }
        [TestMethod]
        public async Task VerifyEmail_Should_Return_BadRequest_When_Verification_Fails()
        {
            // Arrange
            var verifyEmailModel = new VerifyEmailModel();
            _mockUserService.Setup(s => s.VerifyEmailAsync(verifyEmailModel)).ReturnsAsync(false);

            // Act
            var result = await _controller.VerifyEmail(verifyEmailModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(Messages.Controller.InvalidOrExpiredConfirmationCode, badRequestResult.Value);
        }
        
        [TestMethod]
        public async Task Login_Should_Return_Token_When_Credentials_Are_Valid_And_2FA_Not_Required()
        {
            // Arrange
            var loginModel = new LoginModel();
            var user = new AppUser();
            _mockUserService.Setup(s => s.LoginAsync(loginModel)).ReturnsAsync((user, false));
            _mockAuthManager.Setup(a => a.GenerateJwtToken(user)).ReturnsAsync("jwt_token");

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
           // Assert.AreEqual("jwt_token", ((dynamic)okResult.Value).token);
        }
        [TestMethod]
        public async Task Login_Should_Return_2FA_Required_When_2FA_Is_Enabled()
        {
            // Arrange
            var loginModel = new LoginModel();
            var user = new AppUser();
            _mockUserService.Setup(s => s.LoginAsync(loginModel)).ReturnsAsync((user, true));

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
           // Assert.IsTrue(((dynamic)okResult.Value).TwoFactorRequired);
           // Assert.AreEqual(user.Id, ((dynamic)okResult.Value).userId);
        }
        [TestMethod]
        public async Task Login_Should_Return_Unauthorized_When_Credentials_Are_Invalid()
        {
            // Arrange
            var loginModel = new LoginModel();
            _mockUserService.Setup(s => s.LoginAsync(loginModel)).ReturnsAsync((null, false));

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(Messages.Controller.InvalidEmailOrPassword, unauthorizedResult.Value);
        }

        [TestMethod]
        public async Task VerifyPhone_Should_Return_Ok_When_Verification_Is_Successful()
        {
            // Arrange
            var phoneNumber = "1234567890";
            var code = "123456";
            _mockUserService.Setup(s => s.VerifyPhoneAsync(phoneNumber, code)).ReturnsAsync(true);

            // Act
            var result = await _controller.VerifyPhone(phoneNumber, code);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(Messages.Controller.PhoneNumberVerificationSuccess, okResult.Value);
        }

        [TestMethod]
        public async Task VerifyPhone_Should_Return_BadRequest_When_Verification_Fails()
        {
            // Arrange
            var phoneNumber = "1234567890";
            var code = "123456";
            _mockUserService.Setup(s => s.VerifyPhoneAsync(phoneNumber, code)).ReturnsAsync(false);

            // Act
            var result = await _controller.VerifyPhone(phoneNumber, code);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(Messages.Controller.InvalidVerificationCode, badRequestResult.Value);
        }
        [TestMethod]
        public async Task UploadProfilePicture_Should_Return_Ok_When_Upload_Is_Successful()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            _mockUserService.Setup(s => s.UploadProfilePictureAsync(It.IsAny<string>(), fileMock.Object)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UploadProfilePicture(fileMock.Object);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
           // Assert.AreEqual(Messages.Controller.ProfilePictureUpdatedSuccessfully, ((dynamic)okResult.Value).message);
        }
        [TestMethod]
        public async Task UploadProfilePicture_Should_Return_NotFound_When_EntityNotFoundException_Is_Thrown()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            _mockUserService.Setup(s => s.UploadProfilePictureAsync(It.IsAny<string>(), fileMock.Object))
                .ThrowsAsync(new EntityNotFoundException("User not found"));

            // Act
            var result = await _controller.UploadProfilePicture(fileMock.Object);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("User not found", notFoundResult.Value);
        }

        [TestMethod]
        public async Task UploadProfilePicture_Should_Return_BadRequest_When_ArgumentException_Is_Thrown()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            _mockUserService.Setup(s => s.UploadProfilePictureAsync(It.IsAny<string>(), fileMock.Object))
                .ThrowsAsync(new ArgumentException("Invalid file"));

            // Act
            var result = await _controller.UploadProfilePicture(fileMock.Object);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Invalid file", badRequestResult.Value);
        }
        [TestMethod]
        public async Task UpdateUserProfile_Should_Return_Ok_When_Update_Is_Successful()
        {
            // Arrange
            var updateUserModel = new UpdateUserModel();
            _mockUserService.Setup(s => s.UpdateUserProfileAsync(updateUserModel, "user123")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.UpdateUserProfile(updateUserModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(Messages.Controller.UserProfileUpdatedSuccessfully, okResult.Value);
        }
        [TestMethod]
        public async Task UpdateUserProfile_Should_Return_BadRequest_When_Update_Fails()
        {
            // Arrange
            var updateUserModel = new UpdateUserModel();
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Error" });
            _mockUserService.Setup(s => s.UpdateUserProfileAsync(updateUserModel, "user123")).ReturnsAsync(identityResult);

            // Act
            var result = await _controller.UpdateUserProfile(updateUserModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(identityResult.Errors, badRequestResult.Value);
        }
        [TestMethod]
        public async Task Enable2FA_Should_Return_PngImage_When_2FA_Is_Enabled()
        {
            // Arrange
            var user = new AppUser();
            _mockUserService.Setup(s => s.GetUserByIdAsync("user123")).ReturnsAsync(user);
            _mockTwoFactorAuthService.Setup(s => s.GenerateQrCodeImageAsync(user)).ReturnsAsync(new byte[] { });

            // Act
            var result = await _controller.Enable2FA();

            // Assert
            var fileResult = result as FileContentResult;
            Assert.IsNotNull(fileResult);
            Assert.AreEqual("image/png", fileResult.ContentType);
        }
        [TestMethod]
        public async Task Enable2FA_Should_Return_NotFound_When_User_Does_Not_Exist()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByIdAsync("user123")).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.Enable2FA();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(Messages.UserNotFound, notFoundResult.Value);
        }
        [TestMethod]
        public async Task Verify2FA_Should_Return_Ok_When_Verification_Is_Successful()
        {
            // Arrange
            var user = new AppUser();
            var verifyModel = new Verify2FAModel { Code = "123456" };
            _mockUserService.Setup(s => s.GetUserByIdAsync("user123")).ReturnsAsync(user);
            _mockTwoFactorAuthService.Setup(s => s.VerifyTwoFactorCodeAsync(user, verifyModel.Code)).ReturnsAsync(true);

            // Act
            var result = await _controller.Verify2FA(verifyModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(Messages.Controller.TwoFactorAuthenticationEnabled, okResult.Value);
        }
        [TestMethod]
        public async Task Verify2FA_Should_Return_BadRequest_When_Verification_Fails()
        {
            // Arrange
            var user = new AppUser();
            var verifyModel = new Verify2FAModel { Code = "123456" };
            _mockUserService.Setup(s => s.GetUserByIdAsync("user123")).ReturnsAsync(user);
            _mockTwoFactorAuthService.Setup(s => s.VerifyTwoFactorCodeAsync(user, verifyModel.Code)).ReturnsAsync(false);

            // Act
            var result = await _controller.Verify2FA(verifyModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(Messages.Controller.InvalidVerificationCode, badRequestResult.Value);
        }
        [TestMethod]
        public async Task Verify2FA_Should_Return_NotFound_When_User_Does_Not_Exist()
        {
            // Arrange
            var verifyModel = new Verify2FAModel { Code = "123456" };
            _mockUserService.Setup(s => s.GetUserByIdAsync("user123")).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.Verify2FA(verifyModel);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
           // Assert.AreEqual(Messages.UserNotFound, notFoundResult.Value);
        }
        [TestMethod]
        public async Task Login2FA_Should_Return_Token_When_2FA_Is_Successful()
        {
            // Arrange
            var login2FAModel = new Login2FAModel { UserId = "user123", Code = "123456" };
            var user = new AppUser();
            _mockUserService.Setup(s => s.GetUserByIdAsync(login2FAModel.UserId)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.VerifyTwoFactorCodeAsync(user, login2FAModel.Code)).ReturnsAsync(true);
            _mockAuthManager.Setup(a => a.GenerateJwtToken(user)).ReturnsAsync("jwt_token");

            // Act
            var result = await _controller.Login2FA(login2FAModel);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
           // Assert.AreEqual("jwt_token", ((dynamic)okResult.Value).token);
        }
        [TestMethod]
        public async Task Login2FA_Should_Return_Unauthorized_When_2FA_Verification_Fails()
        {
            // Arrange
            var login2FAModel = new Login2FAModel { UserId = "user123", Code = "123456" };
            var user = new AppUser();
            _mockUserService.Setup(s => s.GetUserByIdAsync(login2FAModel.UserId)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.VerifyTwoFactorCodeAsync(user, login2FAModel.Code)).ReturnsAsync(false);

            // Act
            var result = await _controller.Login2FA(login2FAModel);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(Messages.Controller.InvalidVerificationCode, unauthorizedResult.Value);
        }
        [TestMethod]
        public async Task Login2FA_Should_Return_Unauthorized_When_User_Not_Found()
        {
            // Arrange
            var login2FAModel = new Login2FAModel { UserId = "user123", Code = "123456" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(login2FAModel.UserId)).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.Login2FA(login2FAModel);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(Messages.UserNotFound, unauthorizedResult.Value);
        }
        [TestMethod]
        public async Task Disable2FA_Should_Return_Ok_When_2FA_Is_Disabled_Successfully()
        {
            // Arrange
            var user = new AppUser();
            _mockUserService.Setup(s => s.GetUserByIdAsync("user123")).ReturnsAsync(user);

            // Act
            var result = await _controller.Disable2FA();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(Messages.Controller.TwoFactorAuthenticationDisabled, okResult.Value);
        }
        [TestMethod]
        public async Task Disable2FA_Should_Return_NotFound_When_User_Does_Not_Exist()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByIdAsync("user123")).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.Disable2FA();

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(Messages.UserNotFound, notFoundResult.Value);
        }


    }
}
