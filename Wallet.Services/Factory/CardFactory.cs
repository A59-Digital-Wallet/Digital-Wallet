using System.Globalization;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Factory
{
    public class CardFactory : ICardFactory
    {
        public Card Map(CardRequest cardRequest, string userId, CardNetwork cardNetwork)
        {
            DateTime expiryDate = DateTimeHelper.ConvertToDateTime(cardRequest.ExpiryDate);
            return new Card
            {
                CardNumber = cardRequest.CardNumber,
                CardHolderName = cardRequest.CardHolderName,
                ExpiryDate = expiryDate,
                CVV = cardRequest.CVV,
                CardType = cardRequest.CardType,
                CardNetwork = cardNetwork,
                AppUserId = userId,
            };
        }

        public CardResponseDTO Map(Card card)
        {
            return new CardResponseDTO
            {
                CardNumber = MeshCardNumber(card.CardNumber),
                CardHolderName = card.CardHolderName,
                ExpiryDate = card.ExpiryDate,
                CardType = card.CardType,
                CardNetwork = card.CardNetwork,
            };
        }

        private static string MeshCardNumber(string cardNumber)
        {
            string firstPart = cardNumber.Substring(0, 4); // First 4 digits
            string lastPart = cardNumber.Substring(cardNumber.Length - 4, 4); // Last 4 digits

            return $"{firstPart}****{lastPart}";
        }
    }
}
