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

        public async Task<List<ITransaction>> GetTransactionHistoryAsync(int id, int pageIndex, int pageSize)
        {
            var wallet = await this.applicationContext.Wallets
                .Include(w => w.AddMoneyTransactions)
                .Include(w => w.WithdrawMoneyTransactions)
                .Include(w => w.TransferMoneyTransactions)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wallet == null)
            {
                return new List<ITransaction>();
            }

            var history = wallet.AddMoneyTransactions
                .Cast<ITransaction>()
                .Union(wallet.WithdrawMoneyTransactions)
                .Union(wallet.TransferMoneyTransactions)
                .OrderByDescending(t => t.Date) // Assuming transactions have a Date property for sorting
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return history;
        }

        public async Task<UserWallet> GetWalletAsync(int id)
        {
            var wallet = await this.applicationContext.Wallets.FindAsync(id);
            return wallet;
        }

        public async Task<bool> UpdateTransactionHistoryAsync(int id, ITransaction transaction)
        {
            var wallet = await this.applicationContext.Wallets.FindAsync(id);
            switch(transaction.TransactionType)
            {
                case Models.Enums.TransactionType.Transfer:
                    wallet.TransferMoneyTransactions.Add((Transfer)transaction);
                    break;
                case Models.Enums.TransactionType.Add:
                    wallet.AddMoneyTransactions.Add((AddMoney)transaction);
                    break;
                case Models.Enums.TransactionType.Withdraw:
                    wallet.WithdrawMoneyTransactions.Add((Withdraw)transaction);
                    break;
            }
            return this.applicationContext.SaveChanges() > 0;
        }

        public async Task<bool> UpdateWalletSumAsync(int id, double amount)
        {
            var wallet = await this.applicationContext.Wallets.FindAsync(id);
            wallet.Balance += amount;
            
            return this.applicationContext.SaveChanges() > 0; 

        }


    }
}
