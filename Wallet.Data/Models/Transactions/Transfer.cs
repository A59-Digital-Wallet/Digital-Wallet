using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wallet.Data.Models.Transactions
{
    public class Transfer : Transaction
    {
        [Required]
        public int RecipientWalletId { get; set; }

        [ForeignKey("RecipientWalletId")]
        public Wallettt RecipientWallet { get; set; }
    }
}