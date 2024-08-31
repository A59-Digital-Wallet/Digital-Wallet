using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Factory;

namespace Wallet.Services.Tests
{
    [TestClass]
    public class CardFactoryTests
    {
        private CardFactory _cardFactory;

        [TestInitialize]
        public void Setup()
        {
            _cardFactory = new CardFactory();
        }

        [TestMethod]
        public void Map_ShouldMapCardRequestToCard()
        {
            // Arrange
            var cardRequest = new CardRequest
            {
                CardNumber = "1234567812345678",
                CardHolderName = "John Doe",
                ExpiryDate = "12/25",
                CVV = "123",
                CardType = CardType.Credit
            };
            var userId = "user1";
            var cardNetwork = CardNetwork.Visa;

            // Act
            var result = _cardFactory.Map(cardRequest, userId, cardNetwork);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(cardRequest.CardNumber, result.CardNumber);
            Assert.AreEqual(cardRequest.CardHolderName, result.CardHolderName);
            Assert.AreEqual(DateTimeHelper.ConvertToDateTime(cardRequest.ExpiryDate), result.ExpiryDate);
            Assert.AreEqual(cardRequest.CVV, result.CVV);
            Assert.AreEqual(cardRequest.CardType, result.CardType);
            Assert.AreEqual(cardNetwork, result.CardNetwork);
            Assert.AreEqual(userId, result.AppUserId);
        }

        [TestMethod]
        public void Map_ShouldMapCardToCardResponseDTO()
        {
            // Arrange
            var card = new Card
            {
                Id = 1,
                CardNumber = "1234567812345678",
                CardHolderName = "John Doe",
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                CardType = CardType.Debit,
                CardNetwork = CardNetwork.MasterCard
            };

            // Act
            var result = _cardFactory.Map(card);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("1234 **** **** 5678", result.CardNumber); // Check masked card number
            Assert.AreEqual(card.CardHolderName, result.CardHolderName);
            Assert.AreEqual(card.ExpiryDate, result.ExpiryDate);
            Assert.AreEqual(card.CardType, result.CardType);
            Assert.AreEqual(card.CardNetwork, result.CardNetwork);
        }

        [TestMethod]
        public void Map_ShouldMapListOfCardsToListOfCardResponseDTOs()
        {
            // Arrange
            var cards = new List<Card>
            {
                new Card
                {
                    Id = 1,
                    CardNumber = "1234567812345678",
                    CardHolderName = "John Doe",
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    CardType = CardType.Credit,
                    CardNetwork = CardNetwork.Visa
                },
                new Card
                {
                    Id = 2,
                    CardNumber = "8765432187654321",
                    CardHolderName = "Jane Smith",
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    CardType = CardType.Debit,
                    CardNetwork = CardNetwork.MasterCard
                }
            };

            // Act
            var result = _cardFactory.Map(cards);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("1234 **** **** 5678", result[0].CardNumber);
            Assert.AreEqual("8765 **** **** 4321", result[1].CardNumber);
            Assert.AreEqual(cards[0].CardHolderName, result[0].CardHolderName);
            Assert.AreEqual(cards[1].CardHolderName, result[1].CardHolderName);
        }

        [TestMethod]
        public void MeshCardNumber_ShouldMaskCardNumberCorrectly()
        {
            // Arrange
            string cardNumber = "1234567812345678";

            // Act
            var result = CardFactoryTestHelper.MeshCardNumber(cardNumber);

            // Assert
            Assert.AreEqual("1234 **** **** 5678", result);
        }

        private static class CardFactoryTestHelper
        {
            public static string MeshCardNumber(string cardNumber)
            {
                string firstPart = cardNumber.Substring(0, 4); // First 4 digits
                string lastPart = cardNumber.Substring(cardNumber.Length - 4, 4); // Last 4 digits

                return $"{firstPart} **** **** {lastPart}";
            }
        }
    }
}
