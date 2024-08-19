using Wallet.Data.Models.Enums;

namespace Wallet.MVC.Models
{
    public class TransactionViewModel
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Direction { get; set; }
        public string FromWallet { get; set; } // Add this to indicate the origin wallet
        public string ToWallet { get; set; }
        public bool IsRecurring { get; set; } // Indicates if this transaction is recurring
        public RecurrenceInterval? RecurrenceInterval { get; set; }
    }
}
