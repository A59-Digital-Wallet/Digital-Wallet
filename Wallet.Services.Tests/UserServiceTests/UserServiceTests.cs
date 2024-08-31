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

            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<AppUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var userConfirmation = new Mock<IUserConfirmation<AppUser>>();
            _mockSignInManager = new Mock<SignInManager<AppUser>>(_mockUserManager.Object, contextAccessor.Object, userPrincipalFactory.Object, options.Object, logger.Object, schemes.Object, userConfirmation.Object);

            _mockCloudinaryService = new Mock<ICloudinaryService>();
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



    }

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Execute<TResult>(expression);
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public T Current => _inner.Current;
    }

}