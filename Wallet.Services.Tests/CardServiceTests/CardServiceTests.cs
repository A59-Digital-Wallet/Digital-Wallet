using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Validation.CardValidation;
using Wallet.Data.Models.Enums;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class CardServiceTests
    {
        private Mock<ICardRepository> _mockCardRepository;
        private Mock<ICardFactory> _mockCardFactory;
        private Mock<IEncryptionService> _mockEncryptionService;
        private CardValidation _cardValidation;
        private CardService _cardService;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockCardRepository = new Mock<ICardRepository>();
            _mockCardFactory = new Mock<ICardFactory>();
            _mockEncryptionService = new Mock<IEncryptionService>();
            _cardValidation = new CardValidation();

            // Initialize service with mocks
            _cardService = new CardService(_mockCardRepository.Object, _mockCardFactory.Object, _cardValidation, _mockEncryptionService.Object);
        }

        [TestMethod]
        public async Task GetCardsAsync_ShouldThrowEntityNotFoundException_WhenNoCardsFound()
        {
            // Arrange
            var userId = "user1";
            _mockCardRepository.Setup(repo => repo.GetCardsAsync(userId)).ReturnsAsync((List<Card>)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _cardService.GetCardsAsync(userId));
        }

        [TestMethod]
        public async Task GetCardsAsync_ShouldReturnDecryptedCards_WhenCardsExist()
        {
            // Arrange
            var userId = "user1";
            var sampleCards = new List<Card>
            {
                new Card { Id = 1, CardNumber = "encryptedCardNumber1", CVV = "encryptedCVV1", AppUserId = userId },
                new Card { Id = 2, CardNumber = "encryptedCardNumber2", CVV = "encryptedCVV2", AppUserId = userId }
            };

            _mockCardRepository.Setup(repo => repo.GetCardsAsync(userId)).ReturnsAsync(sampleCards);
            _mockEncryptionService.Setup(es => es.DecryptAsync(It.IsAny<string>())).ReturnsAsync((string s) => "decrypted" + s);
            _mockCardFactory.Setup(factory => factory.Map(It.IsAny<List<Card>>())).Returns(new List<CardResponseDTO>());

            // Act
            var result = await _cardService.GetCardsAsync(userId);

            // Assert
            _mockEncryptionService.Verify(es => es.DecryptAsync(It.IsAny<string>()), Times.Exactly(4));
            _mockCardFactory.Verify(factory => factory.Map(It.IsAny<List<Card>>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(List<CardResponseDTO>));
        }

        [TestMethod]
        public async Task AddCardAsync_ShouldThrowArgumentException_WhenValidationFails()
        {
            // Arrange
            var cardRequest = new CardRequest { CardNumber = "invalidCardNumber" }; // invalid card number for testing
            var userId = "user1";

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _cardService.AddCardAsync(cardRequest, userId));
        }

        [TestMethod]
        public async Task AddCardAsync_ShouldThrowInvalidOperationException_WhenCardAlreadyExists()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "4111111111111111", // Valid card number for validation
                CVV = "123", // Valid CVV for the network (Visa)
                CardHolderName = "John Doe", // Valid cardholder name
                ExpiryDate = "12/25" // Valid expiry date format
            };
            var userId = "user1";

            _mockEncryptionService.Setup(es => es.EncryptAsync(cardRequest.CardNumber)).ReturnsAsync("encryptedCardNumber");
            _mockCardRepository.Setup(repo => repo.CardExistsAsync(userId, "encryptedCardNumber")).ReturnsAsync(true); // Simulate card already exists

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _cardService.AddCardAsync(cardRequest, userId));
        }

        [TestMethod]
        public async Task AddCardAsync_ShouldAddCardSuccessfully_WhenValidationPassesAndCardDoesNotExist()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "4111111111111111",
                CVV = "123",
                CardHolderName = "John Doe",
                ExpiryDate = "12/25" // Ensure the format matches the expected format in CardValidation
            };
            var userId = "user1";

            _mockEncryptionService.Setup(es => es.EncryptAsync(cardRequest.CardNumber)).ReturnsAsync("encryptedCardNumber");
            _mockEncryptionService.Setup(es => es.EncryptAsync(cardRequest.CVV)).ReturnsAsync("encryptedCVV");
            _mockCardRepository.Setup(repo => repo.CardExistsAsync(userId, "encryptedCardNumber")).ReturnsAsync(false);
            _mockCardFactory.Setup(factory => factory.Map(It.IsAny<CardRequest>(), userId, It.IsAny<CardNetwork>())).Returns(new Card());
            _mockCardRepository.Setup(repo => repo.AddCardAsync(It.IsAny<Card>())).Returns(Task.CompletedTask);

            // Act
            await _cardService.AddCardAsync(cardRequest, userId);

            // Assert
            _mockCardRepository.Verify(repo => repo.AddCardAsync(It.IsAny<Card>()), Times.Once);
        }

        [TestMethod]
        public async Task GetCardAsync_ShouldThrowAuthorizationException_WhenUserIsNotAuthorized()
        {
            // Arrange
            var cardId = 1;
            var userId = "user1";
            var card = new Card { Id = cardId, AppUserId = "user2" }; // Different user

            _mockCardRepository.Setup(repo => repo.GetCardAsync(cardId)).ReturnsAsync(card);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<AuthorizationException>(() => _cardService.GetCardAsync(cardId, userId));
        }

        [TestMethod]
        public async Task GetCardAsync_ShouldReturnDecryptedCard_WhenUserIsAuthorized()
        {
            // Arrange
            var cardId = 1;
            var userId = "user1";
            var card = new Card { Id = cardId, AppUserId = userId, CardNumber = "encryptedNumber", CVV = "encryptedCVV" };

            _mockCardRepository.Setup(repo => repo.GetCardAsync(cardId)).ReturnsAsync(card);
            _mockEncryptionService.Setup(es => es.DecryptAsync("encryptedNumber")).ReturnsAsync("decryptedNumber");
            _mockEncryptionService.Setup(es => es.DecryptAsync("encryptedCVV")).ReturnsAsync("decryptedCVV");
            _mockCardFactory.Setup(factory => factory.Map(card)).Returns(new CardResponseDTO());

            // Act
            var result = await _cardService.GetCardAsync(cardId, userId);

            // Assert
            Assert.IsNotNull(result);
            _mockEncryptionService.Verify(es => es.DecryptAsync(It.IsAny<string>()), Times.Exactly(2));
            _mockCardFactory.Verify(factory => factory.Map(card), Times.Once);
        }

        [TestMethod]
        public async Task DeleteCardAsync_ShouldThrowEntityNotFoundException_WhenCardDoesNotExist()
        {
            // Arrange
            var cardId = 1;
            var userId = "user1";

            _mockCardRepository.Setup(repo => repo.GetCardAsync(cardId)).ReturnsAsync((Card)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _cardService.DeleteCardAsync(cardId, userId));
        }

        [TestMethod]
        public async Task DeleteCardAsync_ShouldThrowAuthorizationException_WhenUserIsNotAuthorized()
        {
            // Arrange
            var cardId = 1;
            var userId = "user1";
            var card = new Card { Id = cardId, AppUserId = "user2" }; // Different user

            _mockCardRepository.Setup(repo => repo.GetCardAsync(cardId)).ReturnsAsync(card);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<AuthorizationException>(() => _cardService.DeleteCardAsync(cardId, userId));
        }

        [TestMethod]
        public async Task DeleteCardAsync_ShouldDeleteCard_WhenUserIsAuthorized()
        {
            // Arrange
            var cardId = 1;
            var userId = "user1";
            var card = new Card { Id = cardId, AppUserId = userId };

            _mockCardRepository.Setup(repo => repo.GetCardAsync(cardId)).ReturnsAsync(card);
            _mockCardRepository.Setup(repo => repo.DeleteCardAsync(card)).Returns(Task.CompletedTask);

            // Act
            var result = await _cardService.DeleteCardAsync(cardId, userId);

            // Assert
            Assert.IsTrue(result);
            _mockCardRepository.Verify(repo => repo.DeleteCardAsync(card), Times.Once);
        }
    }
}
