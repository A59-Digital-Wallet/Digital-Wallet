using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;


namespace Wallet.Data.Models.Transactions
{
    public class NonTransfer : Transaction
    {
        [Required]
        public int UserCardID { get; set; }

        [ForeignKey("CardId")]
        public Card UserCard { get; set; }

        
    }
}
