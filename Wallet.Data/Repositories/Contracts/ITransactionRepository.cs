using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
namespace Wallet.Data.Repositories.Contracts
{
    public interface ITransactionRepository
    {
        Task CreateTransactionAsync(Transaction transaction);
        Task<Transaction> GetTransactionByIdAsync(int transactionId);       
        Task<IList<Transaction>> GetTransactionsByWalletId(int walletId);
        Task<IList<Transaction>> GetTransactionsByUserId(string userId);
        Task<ICollection<Transaction>> FilterBy(int page, int pageSize, TransactionRequestFilter filterParameters, string userId);

        Task<ICollection<Transaction>> GetRecurringTransactionsDueAsync(DateTime dueDate);
        Task UpdateTransactionAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetTransactionHistoryContactAsync(List<int> userWalletIds, List<int> contactWalletIds);



    }
}
