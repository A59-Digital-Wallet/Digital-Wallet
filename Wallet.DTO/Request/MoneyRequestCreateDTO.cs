using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.DTO.Request
{
    public class MoneyRequestCreateDTO
    {
        public string RecipientId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string RequestedCurrency { get; set; }
    }
}
