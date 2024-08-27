using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;

namespace Wallet.DTO.Response
{
    public class WalletResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public Currency Currency { get; set; }
        public WalletType WalletType { get; set; }
        public List<AppUser> AppUserWallets { get; set; } = new List<AppUser>();
        public string OwnerId {  get; set; }
    }
}
