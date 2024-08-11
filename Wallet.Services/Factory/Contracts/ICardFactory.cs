using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;

namespace Wallet.Services.Factory.Contracts
{
    public interface ICardFactory
    {
        Card Map(CardRequest cardRequest, string userId, CardNetwork cardNetwork);
    }
}
