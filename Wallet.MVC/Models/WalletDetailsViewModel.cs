namespace Wallet.MVC.Models
{
    public class WalletDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string WalletType { get; set; }
        public List<string> Members { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }
    }
}
