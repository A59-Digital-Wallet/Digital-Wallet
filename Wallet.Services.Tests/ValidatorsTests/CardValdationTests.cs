using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Wallet.DTO.Request;
using Wallet.Services.Validation.CardValidation;
using Wallet.Data.Models.Enums;

namespace Wallet.Services.Tests.ValidatorsTests
{
    [TestClass]
    public class CardValidationTests
    {
        private CardValidation _cardValidation;

        [TestInitialize]
        public void Setup()
        {
            _cardValidation = new CardValidation();
        }

        [TestMethod]
        public void Validate_ShouldReturnValidResult_WhenCardRequestIsValid()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "4111111111111111", // Valid Visa card number
                CVV = "123", // Valid CVV
                CardHolderName = "John Doe",
                ExpiryDate = "12/25" // Valid future expiry date
            };

            // Act
            var result = _cardValidation.Validate(cardRequest);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(CardNetwork.Visa, result.CardNetwork);
        }

        [TestMethod]
        public void Validate_ShouldReturnError_WhenCardNumberIsInvalid()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "1234567890123456", // Invalid card number (fails Luhn's Algorithm)
                CVV = "123",
                CardHolderName = "John Doe",
                ExpiryDate = "12/25"
            };

            // Act
            var result = _cardValidation.Validate(cardRequest);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Contains("Invalid card number."));
        }

        [TestMethod]
        public void Validate_ShouldReturnError_WhenCardNetworkIsUnknown()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "0000000000000000", // Unknown card network
                CVV = "123",
                CardHolderName = "John Doe",
                ExpiryDate = "12/25"
            };

            // Act
            var result = _cardValidation.Validate(cardRequest);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Contains("Unknown card network."));
        }

        [TestMethod]
        public void Validate_ShouldReturnError_WhenCardLengthIsInvalid()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "411111111111", // Invalid length for Visa
                CVV = "123",
                CardHolderName = "John Doe",
                ExpiryDate = "12/25"
            };

            // Act
            var result = _cardValidation.Validate(cardRequest);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Contains("Invalid card number length."));
        }

        [TestMethod]
        public void Validate_ShouldReturnError_WhenCardIsExpired()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "4111111111111111",
                CVV = "123",
                CardHolderName = "John Doe",
                ExpiryDate = "01/23" // Expiry date in the past
            };

            // Act
            var result = _cardValidation.Validate(cardRequest);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Contains("The card is nearing its expiration. Please provide a card with more than one month of validity remaining."));
        }

        [TestMethod]
        public void Validate_ShouldReturnError_WhenCVVIsInvalid()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "4111111111111111", // Visa
                CVV = "12", // Invalid CVV length for Visa
                CardHolderName = "John Doe",
                ExpiryDate = "12/25"
            };

            // Act
            var result = _cardValidation.Validate(cardRequest);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Contains("Invalid CVV."));
        }

        [TestMethod]
        public void Validate_ShouldReturnError_WhenCardHolderNameIsInvalid()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "4111111111111111",
                CVV = "123",
                CardHolderName = "John Doe 123", // Invalid name with digits
                ExpiryDate = "12/25"
            };

            // Act
            var result = _cardValidation.Validate(cardRequest);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Contains("Invalid cardholder name."));
        }
    }
}
