using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Implementations;
using Wallet.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Wallet.Data.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CloudinaryDotNet.Actions;
using Wallet.Services.Models;
using Castle.Components.DictionaryAdapter;
using Castle.Core.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<UserManager<AppUser>> _mockUserManager;
        private Mock<ICloudinaryService> _mockCloudinaryService;
        private Mock<SignInManager<AppUser>> _mockSignInManager;
        private Mock<IEmailSender> _mockEmailSender;
        private Mock<ITwoFactorAuthService> _mockTwoFactorAuthService;
        private Mock<IConfiguration> _mockConfiguration;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();

            var mockUserStore = new Mock<IUserStore<AppUser>>();
            var mockOptions = new Mock<IOptions<IdentityOptions>>();
            var mockPasswordHasher = new Mock<IPasswordHasher<AppUser>>();
            var mockUserValidators = new List<IUserValidator<AppUser>> { new Mock<IUserValidator<AppUser>>().Object };
            var mockPasswordValidators = new List<IPasswordValidator<AppUser>> { new Mock<IPasswordValidator<AppUser>>().Object };
            var mockKeyNormalizer = new Mock<ILookupNormalizer>();
            var mockErrors = new Mock<IdentityErrorDescriber>();
            var mockServices = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<UserManager<AppUser>>>();

            _mockUserManager = new Mock<UserManager<AppUser>>(
                mockUserStore.Object,
                mockOptions.Object,
                mockPasswordHasher.Object,
                mockUserValidators,
                mockPasswordValidators,
                mockKeyNormalizer.Object,
                mockErrors.Object,
                mockServices.Object,
                mockLogger.Object);

            _mockCloudinaryService = new Mock<ICloudinaryService>();
            _mockSignInManager = new Mock<SignInManager<AppUser>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<AppUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<AppUser>>().Object);

            _mockEmailSender = new Mock<IEmailSender>();
            _mockTwoFactorAuthService = new Mock<ITwoFactorAuthService>();
            _mockConfiguration = new Mock<IConfiguration>();

            _userService = new UserService(
                _mockUserRepository.Object,
                _mockUserManager.Object,
                _mockCloudinaryService.Object,
                _mockSignInManager.Object,
                _mockEmailSender.Object,
                _mockTwoFactorAuthService.Object,
                _mockConfiguration.Object
            );
        }








        [TestMethod]
        public async Task VerifyEmailAsync_Should_Verify_Email_When_Code_Is_Correct()
        {
            // Arrange
            var verifyEmailModel = new VerifyEmailModel
            {
                Email = "johndoe@example.com",
                Code = "123456"
            };

            var user = new AppUser
            {
                Email = verifyEmailModel.Email,
                EmailConfirmationCode = "123456",
                EmailConfirmationCodeGeneratedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(verifyEmailModel.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.VerifyEmailAsync(verifyEmailModel);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(user.EmailConfirmationCode);
            _mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }


        [TestMethod]
        public async Task SearchUsersAsync_Should_Return_Users_With_Roles()
        {
            // Arrange
            var searchTerm = "John";
            var page = 1;
            var pageSize = 10;
            var user = new AppUser { Id = "user1", UserName = "JohnDoe" };

            _mockUserRepository.Setup(repo => repo.SearchUsersAsync(searchTerm, page, pageSize))
                .ReturnsAsync(new List<AppUser> { user });

            _mockUserRepository.Setup(repo => repo.GetUserCountAsync(searchTerm))
                .ReturnsAsync(1);

            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _userService.SearchUsersAsync(searchTerm, page, pageSize);

            // Assert
            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual("JohnDoe", result.Items.First().User.UserName);
            Assert.IsTrue(result.Items.First().Roles.Contains("User"));
        }


        [TestMethod]
        public async Task ManageRoleAsync_Should_Block_User()
        {
            // Arrange
            var userId = "user1";
            var user = new AppUser { Id = userId };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(false);

            _mockUserManager.Setup(um => um.AddToRoleAsync(user, "Blocked"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ManageRoleAsync(userId, "block");

            // Assert
            Assert.IsTrue(result.Succeeded);
            _mockUserManager.Verify(um => um.AddToRoleAsync(user, "Blocked"), Times.Once);
        }
        [TestMethod]
        public async Task UpdateUserProfileAsync_Should_Update_User_Profile()
        {
            // Arrange
            var userId = "user1";
            var updateUserModel = new UpdateUserModel
            {
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "9876543210"
            };

            var user = new AppUser { Id = userId, PhoneNumber = "1234567890" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateUserProfileAsync(updateUserModel, userId);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("9876543210", user.PhoneNumber);
            Assert.AreEqual("John", user.FirstName);
            Assert.AreEqual("Doe", user.LastName);
        }
        [TestMethod]
        public async Task LoginAsync_Should_Return_User_When_Credentials_Are_Valid()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "johndoe@example.com",
                Password = "Password123!"
            };

            var user = new AppUser { Email = loginModel.Email };

            _mockUserManager.Setup(um => um.FindByEmailAsync(loginModel.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginModel.Password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(um => um.GetTwoFactorEnabledAsync(user))
                .ReturnsAsync(false);

            // Act
            var (resultUser, requiresTwoFactor) = await _userService.LoginAsync(loginModel);

            // Assert
            Assert.IsNotNull(resultUser);
            Assert.IsFalse(requiresTwoFactor);
            Assert.AreEqual(loginModel.Email, resultUser.Email);
        }
        [TestMethod]
        public async Task VerifyTwoFactorCodeAsync_Should_Return_True_When_Code_Is_Correct()
        {
            // Arrange
            var user = new AppUser { Id = "user1" };
            var code = "123456";

            _mockTwoFactorAuthService.Setup(tfa => tfa.VerifyTwoFactorCodeAsync(user, code))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.VerifyTwoFactorCodeAsync(user, code);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RegisterUserAsync_Should_Register_User_And_Send_Confirmation_Email()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe1@example.com",
                UserName = "johndoe1",
                PhoneNumber = "123456789000",
                Password = "Password123!"
            };

            var mockUser = new AppUser
            {
                FirstName = "John",
                LastName = "Doe",
                Email = registerModel.Email,
                UserName = registerModel.UserName,
                PhoneNumber = registerModel.PhoneNumber,
            };

            // Mock finding by email to return null (user does not exist)
            _mockUserManager.Setup(um => um.FindByEmailAsync(registerModel.Email))
                .ReturnsAsync((AppUser)null);

            // Mock user creation
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Mock adding to role
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Mock sending email
            _mockEmailSender.Setup(es => es.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterUserAsync(registerModel);

            // Assert
            Assert.IsTrue(result.Succeeded);
            _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<AppUser>(), registerModel.Password), Times.Once);
            _mockEmailSender.Verify(es => es.SendEmail(It.IsAny<string>(), registerModel.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UploadProfilePictureAsync_Should_Upload_Image_And_Update_Profile_Picture()
        {
            // Arrange
            var userId = "user1";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.Length).Returns(100);

            var uploadResult = new CloudinaryUploadResult { Url = "http://example.com/profile.jpg" };

            _mockCloudinaryService.Setup(cs => cs.UploadImageAsync(fileMock.Object))
                .ReturnsAsync(uploadResult);

            _mockUserRepository.Setup(repo => repo.UpdateProfilePictureAsync(userId, uploadResult.Url.ToString()))
                .ReturnsAsync(true);

            // Act
            await _userService.UploadProfilePictureAsync(userId, fileMock.Object);

            // Assert
            _mockCloudinaryService.Verify(cs => cs.UploadImageAsync(fileMock.Object), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateProfilePictureAsync(userId, uploadResult.Url.ToString()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "No file was uploaded.")]
        public async Task UploadProfilePictureAsync_Should_Throw_Exception_When_File_Is_Null()
        {
            // Act
            await _userService.UploadProfilePictureAsync("user1", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Invalid file format.")]
        public async Task UploadProfilePictureAsync_Should_Throw_Exception_When_File_Format_Is_Invalid()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.txt");
            fileMock.Setup(f => f.Length).Returns(100);

            // Act
            await _userService.UploadProfilePictureAsync("user1", fileMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Error uploading file.")]
        public async Task UploadProfilePictureAsync_Should_Throw_Exception_When_Upload_Fails()
        {
            // Arrange
            var userId = "user1";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.Length).Returns(100);

            _mockCloudinaryService.Setup(cs => cs.UploadImageAsync(fileMock.Object))
                .ReturnsAsync((CloudinaryUploadResult)null);

            // Act
            await _userService.UploadProfilePictureAsync(userId, fileMock.Object);
        }
    }
}

   

