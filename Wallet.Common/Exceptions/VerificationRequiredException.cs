namespace Wallet.Common.Exceptions
{
    public class VerificationRequiredException : Exception
    {
        public string TransactionToken { get; }
        public int? WalletId { get; }
        public decimal? Amount { get; }
        public string? Description { get; }
        public int? RecipientWallet { get; }
        public VerificationRequiredException(string transactionToken, int? walletId = null, decimal? amount = null, string? description = null, int? recipientWallet = null)
        {
            TransactionToken = transactionToken;
            WalletId = walletId;
            Amount = amount;
            Description = description;
            RecipientWallet = recipientWallet;
        }
    }
}
