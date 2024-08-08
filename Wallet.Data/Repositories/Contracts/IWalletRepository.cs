using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;

namespace Wallet.Data.Repositories.Contracts
{
    public interface IWalletRepository
    {
        Task CreateWallet(UserWallet wallet);
        Task<UserWallet> GetWalletAsync(int id);
        Task<bool> UpdateWalletSumAsync(int id, double amount);
        Task<List<ITransaction>> GetTransactionHistoryAsync(int id, int pageIndex, int pageSize);
        Task<bool> UpdateTransactionHistoryAsync(int id, ITransaction transaction);


    }
}
