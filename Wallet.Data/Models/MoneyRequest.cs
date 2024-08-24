using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;

namespace Wallet.Data.Models
{
    public class MoneyRequest
    {
        public int Id { get; set; }
        public string RequesterId { get; set; }
        public AppUser Requester { get; set; }  // Navigation property

        // Foreign key to the recipient user
        public string RecipientId { get; set; }
        public AppUser Recipient { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Currency RequestedCurrency { get; set; }
    }
}
