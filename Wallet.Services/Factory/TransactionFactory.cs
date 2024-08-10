using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.DTO.Request;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Factory
{
    public class TransactionFactory : ITransactionFactory
    {
        public Transaction Map(TransactionRequestModel transactionRequest)
        {
            var transaction = new Transaction
            {
                Amount = transactionRequest.Amount,  // Assuming Transaction.Amount is a double
                WalletId = transactionRequest.WalletId,
                Description = transactionRequest.Description,
                TransactionType = transactionRequest.TransactionType,
                Date = DateTime.UtcNow,
                Status = TransactionStatus.Pending
            };

            // Set the recipient wallet ID if it's a transfer
            if (transactionRequest.TransactionType == TransactionType.Transfer)
            {
                transaction.RecipientWalletId = transactionRequest.RecipientWalletId;
            }

            // Set the card ID if a card is involved
            if (transactionRequest.TransactionType == TransactionType.Withdraw ||
                transactionRequest.TransactionType == TransactionType.Deposit)
            {
                transaction.CardId = transactionRequest.CardId;
            }

            return transaction;
        }
    }
}
