using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Enum;

namespace Wallet.Data.Models.Transactions
{
    public interface ITransaction
    {
        
        public int Id { get; set; }

        
        public double Amount { get; set; }

        
        public DateTime Date { get; set; } 
        public string Description { get; set; }

        
        public TransactionStatus Status { get; set; } 

        
        public int WalletId { get; set; }

        
        public UserWallet Wallet { get; set; }

        public TransactionType TransactionType { get; set; }
    }
}
