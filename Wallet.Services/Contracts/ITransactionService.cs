using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface ITransactionService
    {
        Task CreateTransactionAsync(TransactionRequestModel transactionRequest, string userId, string token = null);
        Task<ICollection<TransactionDto>> FilterTransactionsAsync(int page, int pageSize, TransactionRequestFilter filterParameters, string userID);
        Task<UserWithWalletsDto> SearchUserWithWalletsAsync(string searchTerm);
        Task ProcessRecurringTransactionsAsync();
        Task CancelRecurringTransactionAsync(int transactionId, string userId);
        Task<bool> VerifyTransactionAsync(string transactionToken, string verificationCode);
        Task AddTransactionToCategoryAsync(int transactionId, int categoryId, string userId);
        Task<(List<string> WeekLabels, List<decimal> WeeklySpendingAmounts)> GetWeeklySpendingAsync(int walletId);
    }

}
