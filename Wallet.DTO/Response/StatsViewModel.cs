using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.DTO.Response
{
    public class StatsViewModel
    {
        public decimal TotalBalance { get; set; }
        public string CurrencyCulture { get; set; }
        public List<WalletStatsViewModel> WalletBreakdown { get; set; }
       public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set;}


        public Dictionary<string, decimal> CategoryBreakdown { get; set; }
        
        public List<TransactionDto> RecentTransactions { get; set; }
    }
}
