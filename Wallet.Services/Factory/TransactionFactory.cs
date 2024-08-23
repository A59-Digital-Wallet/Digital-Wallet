using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
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
                Status = TransactionStatus.Pending,
                IsRecurring = transactionRequest.IsRecurring,
                Interval = transactionRequest.RecurrenceInterval,
                
            };

            // Set the recipient wallet ID if it's a transfer
            if (transactionRequest.TransactionType == TransactionType.Transfer)
            {
                transaction.RecipientWalletId = transactionRequest.RecepientWalletId;
            }

            // Set the card ID if a card is involved
            if (transactionRequest.TransactionType == TransactionType.Withdraw ||
                transactionRequest.TransactionType == TransactionType.Deposit)
            {
                transaction.CardId = transactionRequest.CardId;
            }

            return transaction;
        }

        public TransactionDto Map(Transaction transaction)
        {
            
            return new TransactionDto
            {           
                Id = transaction.Id,
                Amount = (decimal)transaction.Amount,
                Date = transaction.Date,
                Description = transaction.Description,
                Status = transaction.Status,
                WalletId = transaction.WalletId,
                WalletName = transaction.Wallet?.Name, // Optional
                TransactionType = transaction.TransactionType,
                RecepientWalledId = transaction.RecipientWalletId,
                RecepientWalledName = transaction.RecipientWallet?.Name,
                IsReccuring = transaction.IsRecurring,
                RecurrenceInterval = transaction.Interval,
                OriginalAmount = transaction.OriginalAmount,
                OriginalCurrency = transaction.OriginalCurrency.ToString(),
               SentCurrency = transaction.SentCurrency.ToString(),

            };



        }
    }
}
