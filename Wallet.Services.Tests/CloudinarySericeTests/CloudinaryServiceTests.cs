using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Wallet.Common.Helpers;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.CloudinaryServiceTests
{
    [TestClass]
    public class CloudinaryServiceTests
    {
        private Mock<CloudinaryHelper> _mockCloudinaryHelper;
        private CloudinaryService _cloudinaryService;

        [TestInitialize]
        public void Setup()
        {
            // Mock the CloudinaryHelper
            _mockCloudinaryHelper = new Mock<CloudinaryHelper>(null);  // We pass null because the actual Cloudinary instance isn't needed in the mock
            _cloudinaryService = new CloudinaryService(_mockCloudinaryHelper.Object);
        }

        [TestMethod]
        public async Task UploadImageAsync_UploadsFileToCloudinary_ReturnsCloudinaryUploadResult()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024); // Non-zero size to simulate a valid file
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024])); // Provide a valid memory stream
            mockFile.Setup(f => f.FileName).Returns("test_image.jpg");

            var expectedUploadResult = new ImageUploadResult
            {
                PublicId = "some-public-id",
                SecureUrl = new Uri("https://res.cloudinary.com/dummy-cloud-name/image/upload/v1662226237/some-public-id.jpg")
            };

            // Mock the UploadImageAsync method of CloudinaryHelper
            _mockCloudinaryHelper
                .Setup(c => c.UploadImageAsync(It.IsAny<ImageUploadParams>()))
                .ReturnsAsync(expectedUploadResult);

            // Act
            var result = await _cloudinaryService.UploadImageAsync(mockFile.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUploadResult.PublicId, result.PublicId);
            Assert.AreEqual(expectedUploadResult.SecureUrl.AbsoluteUri, result.Url);
        }

        [TestMethod]
        public async Task UploadImageAsync_EmptyFile_ReturnsNull()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0); // Empty file

            // Act
            var result = await _cloudinaryService.UploadImageAsync(mockFile.Object);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UploadImageAsync_ThrowsException_RethrowsException()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024])); // Valid memory stream
            mockFile.Setup(f => f.FileName).Returns("test_image.jpg");

            // Simulate throwing an exception from UploadImageAsync
            _mockCloudinaryHelper
                .Setup(c => c.UploadImageAsync(It.IsAny<ImageUploadParams>()))
                .ThrowsAsync(new Exception("Upload failed"));

            // Act
            await _cloudinaryService.UploadImageAsync(mockFile.Object);

            // Assert is handled by the ExpectedException attribute
        }
    }
}
