﻿using System;
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
    public abstract class Transaction : ITransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Amount { get; set; }

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
    }
}
