namespace Wallet.MVC.Models
{
    public class TransactionViewModel
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Direction { get; set; }
    }
}
