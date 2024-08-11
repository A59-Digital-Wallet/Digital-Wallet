using System.Globalization;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
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
                // Need to figure out how to handle user.
            };
        }
    }
}
