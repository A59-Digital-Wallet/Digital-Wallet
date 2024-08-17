using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models;

namespace Wallet.DTO.Response
{
    public class CardResponseDTO
    {
        public int Id {  get; set; }
        public string CardNumber { get; set; }

        public string CardHolderName { get; set; }

        public DateTime ExpiryDate { get; set; }

        public CardType CardType { get; set; }

        public CardNetwork CardNetwork { get; set; }

    }
}
