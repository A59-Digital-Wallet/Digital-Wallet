using Wallet.Data.Models;

namespace Wallet.MVC.Models
{
    public class TransactionHistoryViewModel
    {
        public List<MonthlyTransactionViewModel> MonthlyTransactions { get; set; }
        public TransactionRequestFilter Filter { get; set; } // Include the filter for the form
        public List<WalletViewModel> Wallets { get; set; }
    }
}
