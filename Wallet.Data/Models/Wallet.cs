using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Data.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string Currency {  get; set; }
        public decimal Balance { get; set; }
        public ICollection<Transaction> Transactions { get; set; }

        //public ICollection<User> Users { get; set; } 
    }
}
