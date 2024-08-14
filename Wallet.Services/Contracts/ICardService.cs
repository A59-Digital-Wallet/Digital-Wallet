using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface ICardService
    {
        Task AddCardAsync(CardRequest cardRequest, string userID);
        Task<CardResponseDTO> GetCardAsync(int cardI, string userIDd);
        Task<bool> DeleteCardAsync(int cardId, string userId);
    }
}
