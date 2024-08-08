using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Request;

namespace Wallet.Services.Factory.Contracts
{
    public class WalletFactory : IWalletFactory
    {
        public UserWallet Map(UserWalletRequest request)
        {
            var wallet = new UserWallet
            {
                Name = request.Name,
                Currency = request.Currency,
                Balance = 0
            };

            return wallet;
            
        }
    }
}
