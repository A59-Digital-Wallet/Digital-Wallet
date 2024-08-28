using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.DTO.Request;

namespace Wallet.Services.Validation.TransactionValidation
{
    public class TransactionValidator : ITransactionValidator
    {
        public void ValidateOverdraftAndBalance(UserWallet wallet, decimal transactionAmount)
        {

            if (wallet.IsOverdraftEnabled && wallet.WalletType == WalletType.Personal)
            {
                // Calculate the available balance including the overdraft limit
                var availableBalance = wallet.Balance + wallet.OverdraftLimit;

                if (transactionAmount > availableBalance)
                {
                    throw new InvalidOperationException("Transaction exceeds overdraft limit.");
                }
            }
            else
            {
                // Overdraft is not enabled, so just check the balance
                if (transactionAmount > wallet.Balance)
                {
                    throw new InvalidOperationException("Insufficient funds.");
                }
            }
        }

        public void ValidateWalletOwnership(UserWallet wallet, string userId)
        {
            if (wallet.OwnerId != userId && !wallet.AppUserWallets.Any(uw => uw.Id == userId))
            {
                throw new UnauthorizedAccessException("Not your wallet!");
            }
        }

        public bool IsHighValueTransaction(TransactionRequestModel transactionRequest, UserWallet wallet)
        {
            return (transactionRequest.Amount >= wallet.Balance * 0.8m || transactionRequest.Amount > 20000)
                   && transactionRequest.TransactionType != TransactionType.Deposit;
        }
    }
}
