using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Models.Enums;

namespace Wallet.Data.Models
{

    public class UserWallet
    {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public Currency Currency { get; set; }

            [Required]
            public double Balance { get; set; }

            [Required]
            public string AppUserId { get; set; }

            [ForeignKey("AppUserId")]
            public AppUser AppUser { get; set; }


        public List<NonTransfer> NonTransferTransactions { get; set; }
        public List<Transfer> TransferMoneyTransactions { get; set; }
        
    }
    
}
