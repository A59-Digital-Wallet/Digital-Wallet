using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
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


        public async Task<List<UserWallet>> GetSavingsWalletsAsync()
        {
            return await applicationContext.Wallets
                                 .Where(w => w.WalletType == WalletType.Savings)
                                 .ToListAsync();
        }

        public async Task<List<UserWallet>> GetWalletsForProcessingAsync()
        {
            return await applicationContext.Wallets
                .Where(w => w.IsOverdraftEnabled && w.Balance < 0)
                .ToListAsync();
        }
        public async Task<List<UserWallet>> GetUserWalletsAsync(string userId)
        {
            // Fetch wallets where the user is the owner or a member
            return await applicationContext.Wallets
                .Where(w => w.OwnerId == userId || w.AppUserWallets.Any(uw => uw.Id == userId))
                .Include(w => w.AppUserWallets)
                .ToListAsync();
        }
        public async Task AddMemberToJointWalletAsync(int walletId, AppUser userWallet)
        {
            var wallet = await GetWalletAsync(walletId);          

            wallet.AppUserWallets.Add(userWallet);
            await applicationContext.SaveChangesAsync(); // Save changes
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
            await applicationContext.SaveChangesAsync(); // Save changes
        }

        public async Task<bool> UpdateWalletAsync()
        {
            //applicationContext.Wallets.Update(wallet);
            return await applicationContext.SaveChangesAsync() > 0;
        }





    }
}
