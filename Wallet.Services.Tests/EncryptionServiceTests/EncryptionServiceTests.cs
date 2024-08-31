using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Contracts;
using Wallet.Services.Encryption;

namespace Wallet.Services.Tests.EncryptionServiceTests
{
    [TestClass]
    public class EncryptionServiceTests
    {
        private IEncryptionService _encryptionService;
        private Mock<IConfiguration> _configurationMock;
        private const string TestKey = "0123456789abcdef0123456789abcdef"; // 32 bytes for AES-256
        private const string TestIV = "abcdef0123456789"; // 16 bytes for AES

        [TestInitialize]
        public void Setup()
        {
            // Setup mock configuration
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["Encryption:Key"])
                .Returns(Convert.ToBase64String(Encoding.UTF8.GetBytes(TestKey)));
            _configurationMock.Setup(config => config["Encryption:IV"])
                .Returns(Convert.ToBase64String(Encoding.UTF8.GetBytes(TestIV)));

            // Initialize EncryptionService with mock configuration
            _encryptionService = new EncryptionService(_configurationMock.Object);
        }

        [TestMethod]
        public async Task EncryptAsync_ShouldEncryptTextCorrectly()
        {
            // Arrange
            var plainText = "Hello, World!";

            // Act
            var encryptedText = await _encryptionService.EncryptAsync(plainText);

            // Assert
            Assert.IsNotNull(encryptedText);
            Assert.AreNotEqual(plainText, encryptedText); // Ensure encryption is done
        }

        [TestMethod]
        public async Task DecryptAsync_ShouldDecryptTextCorrectly()
        {
            // Arrange
            var plainText = "Hello, World!";
            var encryptedText = await _encryptionService.EncryptAsync(plainText);

            // Act
            var decryptedText = await _encryptionService.DecryptAsync(encryptedText);

            // Assert
            Assert.IsNotNull(decryptedText);
            Assert.AreEqual(plainText, decryptedText); // Ensure decryption is correct
        }

        [TestMethod]
        public async Task EncryptAsync_ThenDecryptAsync_ShouldReturnOriginalText()
        {
            // Arrange
            var originalText = "Sample text to encrypt and decrypt.";

            // Act
            var encryptedText = await _encryptionService.EncryptAsync(originalText);
            var decryptedText = await _encryptionService.DecryptAsync(encryptedText);

            // Assert
            Assert.AreEqual(originalText, decryptedText);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public async Task DecryptAsync_ShouldThrowException_WhenInvalidCipherText()
        {
            // Arrange
            var invalidCipherText = "InvalidBase64Text";

            // Act
            await _encryptionService.DecryptAsync(invalidCipherText);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public async Task EncryptAsync_ShouldHandleEmptyString()
        {
            // Arrange
            var emptyText = string.Empty;

            // Act
            var encryptedText = await _encryptionService.EncryptAsync(emptyText);
            var decryptedText = await _encryptionService.DecryptAsync(encryptedText);

            // Assert
            Assert.AreEqual(emptyText, decryptedText);
        }
    }
}
