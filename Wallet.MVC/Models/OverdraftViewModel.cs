namespace Wallet.MVC.Models
{
    public class OverdraftViewModel
    {
        public string WalletName { get; set; }
        public int WalletId { get; set; }
        public bool IsOverdraftEnabled { get; set; }
        public decimal OverdraftLimit { get; set; }
        public int ConsecutiveNegativeMonths { get; set; }
        public decimal InterestRate { get; set; }
    }
}
