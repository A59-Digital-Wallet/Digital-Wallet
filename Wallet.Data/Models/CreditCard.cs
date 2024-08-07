using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Data.Models
{
    public class CreditCard
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public string CVV { get; set; }

        [Required]
        public string AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser AppUser { get; set; }

    }
}
