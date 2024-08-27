using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Response
{
    public class TransactionDto
    {

        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public TransactionStatus Status { get; set; }
        public int WalletId { get; set; }
        public string WalletName { get; set; } // Optional, if you want to include wallet info
        public TransactionType TransactionType { get; set; }
        public int? RecepientWalledId { get; set; }
        public string? RecepientWalledName { get; set; }
        public string Direction { get; set; }
        public bool IsReccuring { get; set; }
        public RecurrenceInterval? RecurrenceInterval { get; set; }

        public decimal OriginalAmount { get; set; }
        public string OriginalCurrency { get; set; }
        public string SentCurrency { get; set; }
    }

}
