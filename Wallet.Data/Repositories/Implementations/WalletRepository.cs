using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;

using Wallet.Data.Repositories.Contracts;
using Transaction = Wallet.Data.Models.Transactions.Transaction;

namespace Wallet.Data.Repositories.Implementations
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ApplicationContext applicationContext;

        public WalletRepository(ApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        public async Task CreateWallet(UserWallet wallet)
        {
            applicationContext.Wallets.Add(wallet);
            await applicationContext.SaveChangesAsync();
        }

     

        public async Task<UserWallet> GetWalletAsync(int id)
        {
            var wallet = await this.applicationContext.Wallets.FindAsync(id);
            return wallet;
        }

        public async Task UpdateWalletAsync(UserWallet wallet)
        {
            applicationContext.Wallets.Update(wallet);
            await applicationContext.SaveChangesAsync();
        }





    }
}
