using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Request
{
    public class UserWalletRequest
    {
        public string Name { get; set; }
        public Currency Currency { get; set; }
        public WalletType WalletType { get; set; }
    }
}
