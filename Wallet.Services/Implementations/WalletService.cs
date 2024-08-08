using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Implementations
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWalletFactory _walletFactory;
        

        public WalletService(IWalletRepository walletRepository, UserManager<AppUser> userManager, IWalletFactory walletFactory)
        {
            _walletRepository = walletRepository;
            _userManager = userManager;
            _walletFactory = walletFactory;
        }

        public async Task CreateWallet(UserWalletRequest wallet, string userId)
        {
            var createdWallet = _walletFactory.Map(wallet);
            createdWallet.AppUserId = userId;
            await _walletRepository.CreateWallet(createdWallet);
        }

        public async Task<List<ITransaction>> GetTransactionHistoryAsync(int id, int pageIndex, int pageSize, string userId)
        {
            var wallet = await _walletRepository.GetWalletAsync(id);
            if (wallet == null || wallet.AppUserId != userId)
            {
                throw new UnauthorizedAccessException("User does not have access to this wallet.");
            }

            return await _walletRepository.GetTransactionHistoryAsync(id, pageIndex, pageSize);
        }

      

        public async Task<UserWallet> GetWalletAsync(int id, string userId)
        {
            var wallet = await _walletRepository.GetWalletAsync(id);
            if (wallet == null || wallet.AppUserId != userId)
            {
                throw new UnauthorizedAccessException("User does not have access to this wallet.");
            }

            return wallet;
        }

        public async Task<bool> UpdateTransactionHistoryAsync(int id, ITransaction transaction, AppUser appUser)
        {
            var wallet = await _walletRepository.GetWalletAsync(id);
            if (wallet == null || wallet.AppUserId != appUser.Id)
            {
                throw new UnauthorizedAccessException("User does not have access to this wallet.");
            }

            return await _walletRepository.UpdateTransactionHistoryAsync(id, transaction);
        }

        public async Task<bool> UpdateWalletSumAsync(int id, double amount, AppUser appUser)
        {
            var wallet = await _walletRepository.GetWalletAsync(id);
            if (wallet == null || wallet.AppUserId != appUser.Id)
            {
                throw new UnauthorizedAccessException("User does not have access to this wallet.");
            }

            return await _walletRepository.UpdateWalletSumAsync(id, amount);
        }

        public Task<bool> UpdateWalletSumAsync(int id, double amount, string userID)
        {
            throw new NotImplementedException();
        }
    }
}
