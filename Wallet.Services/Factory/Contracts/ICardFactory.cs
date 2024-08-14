using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Factory.Contracts
{
    public interface ICardFactory
    {
        Card Map(CardRequest cardRequest, string userId, CardNetwork cardNetwork);
        CardResponseDTO Map(Card card);
        List<CardResponseDTO> Map(List<Card> cards);
    }
}
