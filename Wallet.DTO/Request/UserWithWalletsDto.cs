using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.DTO.Request
{
    public class UserWithWalletsDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<WalletDto> Wallets { get; set; }
    }
}
