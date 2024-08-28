using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Response
{
    public class CardResponseDTO
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }

        public string CardHolderName { get; set; }

        public DateTime ExpiryDate { get; set; }

        public CardType CardType { get; set; }

        public CardNetwork CardNetwork { get; set; }

    }
}
