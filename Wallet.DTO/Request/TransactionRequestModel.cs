using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Request
{
    public class TransactionRequestModel
    {
        public decimal Amount { get; set; }
        public int WalletId { get; set; }
        
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }
        public int CardId { get; set; }
        public int RecepientWalletId { get; set; }
    }
}
