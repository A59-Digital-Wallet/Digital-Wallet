using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Transactions;

using Wallet.DTO.Request;
using Wallet.DTO.Response;

namespace Wallet.Services.Factory.Contracts
{
    public interface ITransactionFactory
    {
        Transaction Map(TransactionRequestModel transactionRequest);
        TransactionDto Map(Transaction transaction);

    }
}
