﻿using Wallet.Data.Models;

namespace Wallet.DTO.Request
{
    public class UserWithWalletsDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<WalletDto> Wallets { get; set; }
        public List<Category>? Categories { get; set; }
        public UserWallet PreferredWallet { get; set; }
    }
}
