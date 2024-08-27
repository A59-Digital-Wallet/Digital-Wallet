using Microsoft.AspNetCore.Mvc.Rendering;
using Wallet.Data.Models.Enums;

namespace Wallet.MVC.Models
{
    public class TransferViewModel
    {
        public int FromWalletId { get; set; } // Automatically set from preferred wallet
        public int ToWalletId { get; set; }
        public string ContactId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } // Added Description property

        public string RecipientUsernameOrEmail { get; set; } // For searching recipient
        public List<SelectListItem> PotentialRecipients { get; set; } = new List<SelectListItem>(); // To hold potential recipient options
        public string SelectedRecipientId { get; set; } // To hold the selected recipient's ID
        public List<SelectListItem> RecipientWallets { get; set; } = new List<SelectListItem>(); // To hold the recipient's wallets
        public Currency Currency { get; set; }
        public bool IsRecurring { get; set; }
        public RecurrenceInterval? RecurrenceInterval { get; set; }
        public int? SelectedCategoryId { get; set; }
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
