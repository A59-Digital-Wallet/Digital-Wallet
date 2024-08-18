namespace Wallet.MVC.Models
{
    public class MonthlyTransactionViewModel
    {
        public string MonthYear { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }
    }
}
