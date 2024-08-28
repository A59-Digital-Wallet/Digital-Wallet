using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Request
{
    public class WalletDto
    {
        public int WalletId { get; set; }
        public Currency Currency { get; set; }
        public decimal Balance { get; set; }
    }
}
