using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.VerifyEmailServiceTests
{
    [TestClass]
    public class VerifyEmailServiceTests
    {
        private Mock<IEmailSender> _mockEmailSender;
        private VerifyEmailService _verifyEmailService;

        [TestInitialize]
        public void Setup()
        {
            _mockEmailSender = new Mock<IEmailSender>();
            _verifyEmailService = new VerifyEmailService(_mockEmailSender.Object);
        }

        [TestMethod]
        public async Task SendVerificationCodeAsync_Should_Send_Email_With_Code()
        {
            // Arrange
            var email = "test@example.com";
            var username = "TestUser";

            // Act
            var result = await _verifyEmailService.SendVerificationCodeAsync(email, username);

            // Assert
            Assert.IsTrue(result);
            _mockEmailSender.Verify(sender => sender.SendEmail(
                It.Is<string>(subject => subject.Contains("Verification Code")),
                email,
                username,
                It.Is<string>(message => message.Contains("Hello TestUser") && message.Contains("Your verification code is:"))
                
            ), Times.Once);
        }

        [TestMethod]
        public void VerifyCode_Should_Return_True_When_Code_Is_Correct()
        {
            // Arrange
            var email = "test@example.com";
            var username = "TestUser";
            var verificationCode = "123456";

            // Simulate sending the code
            _verifyEmailService.SendVerificationCodeAsync(email, username).Wait();

            // Manually set the code for testing
            _verifyEmailService.SendVerificationCodeAsync(email, username).Wait();
            var field = _verifyEmailService.GetType().GetField("_verificationCodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var verificationCodes = (ConcurrentDictionary<string, string>)field.GetValue(_verifyEmailService);
            verificationCodes[email] = verificationCode;

            // Act
            var result = _verifyEmailService.VerifyCode(email, verificationCode);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerifyCode_Should_Return_False_When_Code_Is_Incorrect()
        {
            // Arrange
            var email = "test@example.com";
            var username = "TestUser";
            var correctCode = "123456";
            var incorrectCode = "654321";

            // Simulate sending the code
            _verifyEmailService.SendVerificationCodeAsync(email, username).Wait();

            // Manually set the code for testing
            var field = _verifyEmailService.GetType().GetField("_verificationCodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var verificationCodes = (ConcurrentDictionary<string, string>)field.GetValue(_verifyEmailService);
            verificationCodes[email] = correctCode;

            // Act
            var result = _verifyEmailService.VerifyCode(email, incorrectCode);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
