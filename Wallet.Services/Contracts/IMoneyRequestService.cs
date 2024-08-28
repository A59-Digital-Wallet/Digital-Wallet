using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface IMoneyRequestService
    {
        Task<MoneyRequestResponseDTO> CreateMoneyRequestAsync(MoneyRequestCreateDTO requestDto, string requesterId);
        Task<IEnumerable<MoneyRequestResponseDTO>> GetReceivedRequestsAsync(string recipientId);
        Task<MoneyRequestResponseDTO> GetMoneyRequestByIdAsync(int id);
        Task UpdateMoneyRequestStatusAsync(int id, RequestStatus status);
        Task ApproveMoneyRequestAsync(int requestId, string senderId);
    }
}
