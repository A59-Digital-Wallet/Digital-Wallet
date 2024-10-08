﻿using System.ComponentModel.DataAnnotations;
using Wallet.Common.Helpers;
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
        [ExpiryDate(ErrorMessage = "Expiry date must be in MM/yy format.")]
        public string ExpiryDate { get; set; }

        [Required]
        [MaxLength(4)]
        public string CVV { get; set; }

        [Required]
        public CardType CardType { get; set; }
    }
}
