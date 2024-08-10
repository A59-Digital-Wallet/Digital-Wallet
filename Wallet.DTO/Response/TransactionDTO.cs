using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Response
{
    public class TransactionDto
    {
        
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public TransactionStatus Status { get; set; }
        public int WalletId { get; set; }
        public string WalletName { get; set; } // Optional, if you want to include wallet info
        public TransactionType TransactionType { get; set; }
    }

}
