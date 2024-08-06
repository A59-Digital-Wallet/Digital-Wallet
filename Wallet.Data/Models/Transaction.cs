using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Data.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public decimal Amount {  get; set; }
        public string Status {  get; set; } //Pending, Completed, Failed, maybe an enum?
        public string Description { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet {  get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
