﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Transactions;

namespace Wallet.Data.Models
{

    public class Wallettt
    {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Currency { get; set; }

            [Required]
            public decimal Balance { get; set; }

            [Required]
            public string AppUserId { get; set; }

            [ForeignKey("AppUserId")]
            public AppUser AppUser { get; set; }

            public List<Transaction> Transactions { get; set; }
    }
    
}
