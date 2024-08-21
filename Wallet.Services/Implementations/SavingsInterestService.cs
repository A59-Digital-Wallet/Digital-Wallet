using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Services.Implementations
{
    public class SavingsInterestService
    {
        private readonly IWalletRepository _walletRepository;

        public SavingsInterestService(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task ApplyMonthlyInterestAsync()
        {
            var savingsWallets = await _walletRepository.GetSavingsWalletsAsync();

            foreach (var wallet in savingsWallets)
            {
                wallet.Balance += wallet.Balance * 0.046m / 12; // 4.6% annual interest, applied monthly
                await _walletRepository.UpdateWalletAsync();
            }
        }
    }

}
