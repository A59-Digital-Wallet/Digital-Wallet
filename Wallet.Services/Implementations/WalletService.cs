using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
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

        public async Task AddMemberToJointWalletAsync(int walletId, string userId, bool canSpend, bool canAddFunds, string ownerId)
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if(wallet.WalletType != WalletType.Joint)
            {
                throw new UnauthorizedAccessException("Can't add to wallet that is not joint");
            }
            if(wallet.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Only owners can do that");
            }
            var userWallet = await _userManager.FindByIdAsync(ownerId);
            var user = await _userManager.FindByIdAsync(userId);
            user.JointWallets.Add(wallet);
            await _walletRepository.AddMemberToJointWalletAsync(walletId, userWallet);
        }

        public async Task CreateWallet(UserWalletRequest wallet, string userId)
        {

            if (wallet.Currency == Currency.None)
            {
                throw new ArgumentException("Invalid currency selected.");
            }
            var createdWallet = _walletFactory.Map(wallet);
            createdWallet.OwnerId = userId;
            await _walletRepository.CreateWallet(createdWallet);
        }

      

      

        public async Task<UserWallet> GetWalletAsync(int id, string userId)
        {
            var wallet = await _walletRepository.GetWalletAsync(id);
            var isOwnerOrMember = wallet.OwnerId == userId || wallet.AppUserWallets.Any(uw => uw.Id == userId);

            if (!isOwnerOrMember)
            {
                throw new UnauthorizedAccessException("You do not have access to this wallet.");
            }

            return wallet;
        }

        public async Task RemoveMemberFromJointWalletAsync(int walletId, string userId, string ownerId)
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Only owners can do that");
            }
            var userWallet = wallet.AppUserWallets.SingleOrDefault(uw => uw.Id == userId);

            if (userWallet == null)
            {
                throw new InvalidOperationException("User is not a member of this wallet.");
            }
            var user = await _userManager.FindByIdAsync(userId);
            user.JointWallets.Remove(wallet);

            await _walletRepository.RemoveMemberFromJointWalletAsync(walletId, userWallet);
        }
    }
}
