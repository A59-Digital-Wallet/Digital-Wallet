using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface IWalletService
    {
        Task CreateWallet(UserWalletRequest wallet, string userID);
        Task<WalletResponseDTO> GetWalletAsync(int id, string userID);
        Task AddMemberToJointWalletAsync(int walletId, string userId, bool canSpend, bool canAddFunds, string ownerId);
        Task RemoveMemberFromJointWalletAsync(int walletId, string userId, string ownerId);
        Task ToggleOverdraftAsync(int walletId, string userId);
        Task<List<UserWallet>> GetUserWalletsAsync(string userId);
        Task<List<UserWallet>> GetWalletsForProcessingAsync();
        Task UpdateWalletAsync();
        Task<List<AppUser>> GetWalletMembersAsync(int walletId);
    }
}
