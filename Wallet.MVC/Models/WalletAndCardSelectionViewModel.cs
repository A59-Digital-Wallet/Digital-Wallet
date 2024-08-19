using Microsoft.AspNetCore.Mvc.Rendering;
using Wallet.Data.Models.Enums;

namespace Wallet.MVC.Models
{
    public class WalletAndCardSelectionViewModel
    {
        public string TransactionType { get; set; }
        public int SelectedWalletId { get; set; }
        public string SelectedCardId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public List<SelectListItem>? Wallets { get; set; }
        public List<SelectListItem>? Cards { get; set; }
        public bool IsRecurring { get; set; } // Indicates if this transaction is recurring
        public RecurrenceInterval? RecurrenceInterval { get; set; }
    }

}
