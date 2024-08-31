using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Services.Tests.Mocks
{
    public class MockCardRepository
    {
        private List<Card> sampleCards;

        public Mock<ICardRepository> GetMockRepository()
        {
            var mockRepository = new Mock<ICardRepository>();

            // Sample data for testing
            sampleCards = new List<Card>
            {
                new Card
                {
                    Id = 1,
                    CardHolderName = "John Doe",
                    CardNumber = "1234567890123456",
                    ExpiryDate = new DateTime(2025, 12, 31),
                    CVV = "123",
                    AppUserId = "user1"
                },
                new Card
                {
                    Id = 2,
                    CardHolderName = "Jane Smith",
                    CardNumber = "6543210987654321",
                    ExpiryDate = new DateTime(2024, 11, 30),
                    CVV = "456",
                    AppUserId = "user2"
                }
            };

            // Mock GetCardsAsync
            mockRepository.Setup(x => x.GetCardsAsync(It.IsAny<string>()))
                .ReturnsAsync((string userId) => sampleCards.Where(c => c.AppUserId == userId).ToList());

            // Mock GetCardAsync
            mockRepository.Setup(x => x.GetCardAsync(It.IsAny<int>()))
                .ReturnsAsync((int cardId) => sampleCards.FirstOrDefault(c => c.Id == cardId));

            // Mock AddCardAsync
            mockRepository.Setup(x => x.AddCardAsync(It.IsAny<Card>()))
                .Callback((Card card) =>
                {
                    card.Id = sampleCards.Max(c => c.Id) + 1; // Assign a new ID
                    sampleCards.Add(card);
                });

            // Mock CardExistsAsync
            mockRepository.Setup(x => x.CardExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string userId, string encryptedCardNumber) => sampleCards.Any(c => c.AppUserId == userId && c.CardNumber == encryptedCardNumber));

            // Mock DeleteCardAsync
            mockRepository.Setup(x => x.DeleteCardAsync(It.IsAny<Card>()))
                .Callback((Card card) => sampleCards.Remove(card))
                .Returns(Task.CompletedTask);

            return mockRepository;
        }
    }
}
