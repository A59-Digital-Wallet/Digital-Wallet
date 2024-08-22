using Wallet.Data.Models.Enums;

namespace Wallet.MVC.Models
{
    public class WalletViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public string CurrencyCulture {  get; set; }
    }

}
