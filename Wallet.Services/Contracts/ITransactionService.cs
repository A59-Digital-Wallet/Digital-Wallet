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
        Task CreateTransactionAsync(TransactionRequestModel transactionRequest, string userId);
        Task<ICollection<TransactionDto>> FilterTransactionsAsync(int page, int pageSize, TransactionRequestFilter filterParameters, string userID);
        Task<UserWithWalletsDto> SearchUserWithWalletsAsync(string searchTerm);
        Task ProcessRecurringTransactionsAsync();
        Task CancelRecurringTransactionAsync(int transactionId, string userId);
    }

}
