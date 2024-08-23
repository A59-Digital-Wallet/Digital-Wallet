using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;


namespace Wallet.Data.Models.Transactions
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Description { get; set; }

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required]
        public int WalletId { get; set; }

        [ForeignKey("WalletId")]
        public UserWallet Wallet { get; set; }

        public TransactionType TransactionType { get; set; }

        public int? RecipientWalletId { get; set; } // For transfers

        [ForeignKey("RecipientWalletId")]
        public UserWallet? RecipientWallet { get; set; }

        public int? CardId { get; set; } // For withdrawals or deposits using a credit card

        [ForeignKey("CardId")]
        public Card? Card { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public bool IsRecurring { get; set; } // Indicates if this is a recurring transaction
        public RecurrenceInterval? Interval { get; set; } // Daily, Weekly, etc.
        public DateTime? NextExecutionDate { get; set; } // Date for the next recurrence
        public bool IsActive { get; set; } // Indicates if the recurrence is still active
        public DateTime? LastExecutedDate { get; set; }

        public decimal OriginalAmount { get; set; }
        public Currency OriginalCurrency { get; set; }
        public Currency SentCurrency { get; set; }



    }
}
