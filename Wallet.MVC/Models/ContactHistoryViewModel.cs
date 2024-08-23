using Wallet.Data.Models.Transactions;

namespace Wallet.MVC.Models
{
    public class ContactHistoryViewModel
    {
        public string ContactId { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public List<int> UserWalletIds { get; set; }
        
    }

}
