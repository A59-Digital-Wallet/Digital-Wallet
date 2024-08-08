using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Factory
{
    public class CardFactory : ICardFactory
    {
        public Card Map(CardRequest cardRequest)
        {
            return new Card
            {
                CardNumber = cardRequest.CardNumber,
                CardHolderName = cardRequest.CardHolderName,
                ExpiryDate = cardRequest.ExpiryDate,
                CVV = cardRequest.CVV,
                CardType = cardRequest.CardType,
                // Need to figure out how to handle user.
            };
        }
    }
}
