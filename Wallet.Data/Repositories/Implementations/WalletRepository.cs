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

      

        public async Task AddMemberToJointWalletAsync(int walletId, AppUser userWallet)
        {
            var wallet = await GetWalletAsync(walletId);          

            wallet.AppUserWallets.Add(userWallet);
            await UpdateWalletAsync(wallet);
        }

        public async Task CreateWallet(UserWallet wallet)
        {
            applicationContext.Wallets.Add(wallet);
            await applicationContext.SaveChangesAsync();
        }

        public Task<AppUser> GetUserWalletAsync(int walletId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserWallet> GetWalletAsync(int id)
        {
            var wallet = await this.applicationContext.Wallets
                .Include(wallet => wallet.Owner)
                .Include(wallet => wallet.AppUserWallets)
                .FirstOrDefaultAsync(w => w.Id == id);
                
            return wallet;
        }

        public async Task RemoveMemberFromJointWalletAsync(int walletId, AppUser userWallet)
        {
            var wallet = await GetWalletAsync(walletId);                       
            wallet.AppUserWallets.Remove(userWallet);
            await UpdateWalletAsync(wallet);
        }

        public async Task UpdateWalletAsync(UserWallet wallet)
        {
            applicationContext.Wallets.Update(wallet);
            await applicationContext.SaveChangesAsync();
        }





    }
}
