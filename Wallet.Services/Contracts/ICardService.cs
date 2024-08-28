using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface ICardService
    {
        Task AddCardAsync(CardRequest cardRequest, string userID);
        Task<CardResponseDTO> GetCardAsync(int cardI, string userIDd);
        Task<List<CardResponseDTO>> GetCardsAsync(string userId);
        Task<bool> DeleteCardAsync(int cardId, string userId);
    }
}
