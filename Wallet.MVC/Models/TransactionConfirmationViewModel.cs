namespace Wallet.MVC.Models
{
    public class TransactionConfirmationViewModel
    {
        public int WalletId { get; set; }
        public int CardId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string TransactionType { get; set; }
        public string TransactionToken { get; set; }
        public string VerificationCode { get; set; }
        public int? RecipinetWalletId { get; set; }
    }
}
