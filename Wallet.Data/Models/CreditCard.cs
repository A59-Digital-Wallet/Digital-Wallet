using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Data.Models
{
    public class CreditCard
    {
        
        public int CreditCardId { get; set; }
        public string CardNumber { get; set; }

        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string CardHolder { get; set; }
        public int CheckNumber { get; set; }

    }
}
