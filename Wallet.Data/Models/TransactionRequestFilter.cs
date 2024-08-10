using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;

namespace Wallet.Data.Models
{
    public class TransactionRequestFilter
    {
        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }      
        public DateTime? EndDate { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get;set; }
        public int WalletId { get; set; }
        public Currency Currency { get; set; }
        public string? OrderBy { get;set; } 
        public string? SortBy { get;set;}

        

       
    }
}
