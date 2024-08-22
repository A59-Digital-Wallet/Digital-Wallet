using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Factory
{
    public class WalletFactory : IWalletFactory
    {
        public UserWallet Map(UserWalletRequest request, OverdraftSettings overdraft)
        {
            var wallet = new UserWallet
            {
                Name = request.Name,
                Currency = request.Currency,
                Balance = 0,
                AppUserWallets = new List<AppUser>(),
                WalletType = request.WalletType,
                InterestRate = overdraft.DefaultInterestRate,
                OverdraftLimit = overdraft.DefaultOverdraftLimit,
            };
            return wallet;
        }

        public WalletDto Map(UserWallet userWallet)
        {
            return new WalletDto
            {
                WalletId = userWallet.Id,
                Currency = userWallet.Currency,
                Balance = userWallet.Balance
            };
        }
    }
}
