using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Transaction = Wallet.Data.Models.Transactions.Transaction;


namespace Wallet.Data.Repositories.Contracts
{
    public interface IWalletRepository
    {
        Task CreateWallet(UserWallet wallet);
        Task<UserWallet> GetWalletAsync(int id);
        Task UpdateWalletAsync(UserWallet wallet);





    }
}
