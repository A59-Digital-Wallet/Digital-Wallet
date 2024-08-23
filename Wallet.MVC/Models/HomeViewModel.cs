using Wallet.DTO.Response;

namespace Wallet.MVC.Models
{
    public class HomeViewModel
    {
        public List<WalletViewModel>? Wallets { get; set; }
        public CardResponseDTO? Card { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }
        public List<ContactResponseDTO> Contacts { get; set; }

        public List<string> WeeklySpendingLabels { get; set; }  // Labels for each week
        public List<decimal> WeeklySpendingAmounts { get; set; } // Spending amounts for each week

        public decimal TotalSpentThisMonth { get; set; } // Total spending this month
        public List<CategoryViewModel> Categories { get; set; } 
    }
}
