using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Data.Models.Transactions
{
    public class AddMoney : Transaction
    {
        [Required]
        public int CreditCardId { get; set; }

        [ForeignKey("CreditCardId")]
        public CreditCard CreditCard { get; set; }
    }
}
