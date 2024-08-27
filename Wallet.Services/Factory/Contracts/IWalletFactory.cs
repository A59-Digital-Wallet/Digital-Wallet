using Wallet.Data.Models;
using Wallet.DTO.Request;

namespace Wallet.Services.Factory.Contracts
{
    public interface IWalletFactory
    {
        UserWallet Map(UserWalletRequest request, OverdraftSettings overdraft);
        //AppUserWallet Map(int walletId, string userId, bool canSpend, bool canAddFunds);
        WalletDto Map(UserWallet userWallet);
    }
}
