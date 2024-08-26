using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Response
{
    public class WalletStatsViewModel
    {
        public string WalletName { get; set; }
        public Currency Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal NetIncome { get; set; }
        public decimal TotalTransfersSent { get; set; }
        public decimal TotalTransfersReceived { get; set; }
    }
}
