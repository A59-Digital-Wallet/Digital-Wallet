using Moq;
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

namespace Wallet.Services.Tests
{
    public class MockTransactionFactory
    {
        public Mock<ITransactionFactory> GetMockFactory()
        {
            var mockFactory = new Mock<ITransactionFactory>();

            // Mock the Map method for TransactionRequestModel to Transaction
            mockFactory.Setup(factory => factory.Map(It.IsAny<TransactionRequestModel>()))
                       .Returns((TransactionRequestModel request) => new Transaction
                       {
                           Amount = request.Amount,
                           WalletId = request.WalletId,
                           Description = request.Description,
                           TransactionType = request.TransactionType,
                           Date = DateTime.UtcNow,
                           Status = TransactionStatus.Pending,
                           IsRecurring = request.IsRecurring,
                           Interval = request.RecurrenceInterval,
                           RecipientWalletId = request.TransactionType == TransactionType.Transfer ? request.RecepientWalletId : null,
                           CardId = (request.TransactionType == TransactionType.Withdraw || request.TransactionType == TransactionType.Deposit) ? request.CardId : null
                       });

            // Mock the Map method for Transaction to TransactionDto
            mockFactory.Setup(factory => factory.Map(It.IsAny<Transaction>()))
                       .Returns((Transaction transaction) => new TransactionDto
                       {
                           Id = transaction.Id,
                           Amount = (decimal)transaction.Amount,
                           Date = transaction.Date,
                           Description = transaction.Description,
                           Status = transaction.Status,
                           WalletId = transaction.WalletId,
                           WalletName = transaction.Wallet?.Name,
                           TransactionType = transaction.TransactionType,
                           RecepientWalledId = transaction.RecipientWalletId,
                           RecepientWalledName = transaction.RecipientWallet?.Name,
                           IsReccuring = transaction.IsRecurring,
                           RecurrenceInterval = transaction.Interval,
                           OriginalAmount = transaction.OriginalAmount,
                           OriginalCurrency = transaction.OriginalCurrency.ToString(),
                           SentCurrency = transaction.SentCurrency.ToString(),
                       });

            return mockFactory;
        }
    }
}
