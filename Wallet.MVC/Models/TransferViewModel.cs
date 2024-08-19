using Microsoft.AspNetCore.Mvc.Rendering;
using Wallet.Data.Models.Enums;

namespace Wallet.MVC.Models
{
    public class TransferViewModel
    {
        public int FromWalletId { get; set; }
        public int ToWalletId { get; set; }
        public decimal Amount { get; set; }

        public List<SelectListItem> Wallets { get; set; } = new List<SelectListItem>();
        public string RecipientUsername { get; set; } // To enter the recipient's username
        public List<SelectListItem> RecipientWallets { get; set; } = new List<SelectListItem>(); // To hold the recipient's wallets
        public bool IsRecurring { get; set; } // Indicates if this transaction is recurring
        public RecurrenceInterval? RecurrenceInterval { get; set; }
    }
}
