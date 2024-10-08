﻿using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Request
{
    public class TransactionRequestModel
    {
        public decimal Amount { get; set; }
        public int WalletId { get; set; }

        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }
        public int CardId { get; set; }
        public int? RecepientWalletId { get; set; }

        public bool IsRecurring { get; set; } // Indicates if this transaction is recurring
        public RecurrenceInterval? RecurrenceInterval { get; set; }

        public string Token { get; set; }
        public int? CategoryId { get; set; }
    }
}
