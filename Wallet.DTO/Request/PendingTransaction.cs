using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.DTO.Request
{
    public class PendingTransaction
    {
        public TransactionRequestModel TransactionRequest { get; set; }
        public UserWallet Wallet { get; set; }
        public string UserId { get; set; }
        public string VerificationCode { get; set; }    
    }
}
