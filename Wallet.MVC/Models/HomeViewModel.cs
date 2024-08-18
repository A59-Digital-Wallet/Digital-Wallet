using Wallet.DTO.Response;

namespace Wallet.MVC.Models
{
    public class HomeViewModel
    {
        public List<WalletViewModel>? Wallets { get; set; }
        public CardResponseDTO? Card { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }
        public List<ContactResponseDTO> Contacts { get; set; }
    }
}
