using Wallet.Data.Models;
using Wallet.DTO.Request;

namespace Wallet.Services.Validation.TransactionValidation
{
    public interface ITransactionValidator
    {
        bool IsHighValueTransaction(TransactionRequestModel transactionRequest, UserWallet wallet);
        void ValidateOverdraftAndBalance(UserWallet wallet, decimal transactionAmount);
        void ValidateWalletOwnership(UserWallet wallet, string userId);
    }
}