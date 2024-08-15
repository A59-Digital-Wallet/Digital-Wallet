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
        public decimal Balance { get; set; }

        [Required]
        public WalletType WalletType { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public AppUser? Owner { get; set; }

        public List<AppUser> AppUserWallets { get; set; }

        public List<Transaction> Transactions { get; set; }

        public bool IsOverdraftEnabled  { get; set; } = false;
        public decimal OverdraftLimit { get; set; } = 500m; 
        public int ConsecutiveNegativeMonths { get; set; } = 0;
        public decimal InterestRate { get; set; } = 0.05m; //Default rate is 5%



    }

}
