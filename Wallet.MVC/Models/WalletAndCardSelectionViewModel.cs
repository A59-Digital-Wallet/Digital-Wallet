using Microsoft.AspNetCore.Mvc.Rendering;

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
    }

}
