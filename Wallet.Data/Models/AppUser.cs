using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Transactions;

namespace Wallet.Data.Models
{
    public class AppUser : IdentityUser
    {
        
        [Required]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be 10 digits.")]
        public string PhoneNumber { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName{ get; set; }

        public List<Card> Cards { get; set; }

        public List<UserWallet> OwnedWallets { get; set; } = new List<UserWallet>();

        // Separate navigation property for wallets where the user is an associated user
        public List<UserWallet> JointWallets { get; set; } = new List<UserWallet>();
        public string? EmailConfirmationCode { get; set; }
        public DateTime? EmailConfirmationCodeGeneratedAt { get; set; }
        public string? ProfilePictureURL { get; set; }
        

    }
}
