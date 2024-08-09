using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Request;

using Wallet.Data.Models.Transactions;

namespace Wallet.Services.Contracts
{
    public interface IWalletService
    {
        Task CreateWallet(UserWalletRequest wallet, string userID);
        Task<UserWallet> GetWalletAsync(int id, string userID);
        Task<bool> UpdateWalletSumAsync(int id, double amount, AppUser userID);
        Task<List<ITransaction>> GetTransactionHistoryAsync(int id, int pageIndex, int pageSize, string appUser);
        Task<bool> UpdateTransactionHistoryAsync(int id, ITransaction transaction, AppUser appUser);
    }
}
