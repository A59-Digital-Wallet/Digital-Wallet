using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Data.Models
{
    public class OverdraftSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal DefaultInterestRate { get; set; } = 0.05m; // Default rate is 5%

        [Required]
        public decimal DefaultOverdraftLimit { get; set; } = 500m; // Default overdraft limit

        [Required]
        public int DefaultConsecutiveNegativeMonths { get; set; } = 3; // Default number of consecutive months before blocking     
    }
}
