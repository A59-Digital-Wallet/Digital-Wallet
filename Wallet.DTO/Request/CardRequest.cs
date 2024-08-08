using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using Wallet.Data.Models.Enum;

namespace Wallet.DTO.Request
{
    public class CardRequest
    {
        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [MaxLength(4)]
        public string CVV { get; set; }

        [Required]
        public CardType CardType { get; set; }
    }
}
