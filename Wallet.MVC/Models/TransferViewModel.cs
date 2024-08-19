using Microsoft.AspNetCore.Mvc.Rendering;

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
    }
}
