using Microsoft.AspNetCore.Identity;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Implementations
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWalletFactory _walletFactory;
        private readonly IOverdraftSettingsRepository _overdraftSettingsRepository;


        public WalletService(IWalletRepository walletRepository, UserManager<AppUser> userManager, IWalletFactory walletFactory, IOverdraftSettingsRepository overdraftSettingsRepository)
        {
            _walletRepository = walletRepository;
            _userManager = userManager;
            _walletFactory = walletFactory;
            _overdraftSettingsRepository = overdraftSettingsRepository;
        }

        public async Task AddMemberToJointWalletAsync(int walletId, string userId, bool canSpend, bool canAddFunds, string ownerId)
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet.WalletType != WalletType.Joint)
            {
                throw new UnauthorizedAccessException(Messages.Service.NotJointWallet);
            }
            if (wallet.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException(Messages.Unauthorized);
            }
            var userWallet = await _userManager.FindByIdAsync(ownerId);
            var user = await _userManager.FindByIdAsync(userId);
            user.JointWallets.Add(wallet);
            await _walletRepository.AddMemberToJointWalletAsync(walletId, userWallet);
        }

        public async Task CreateWallet(UserWalletRequest wallet, string userId)
        {
            var overdraftSettings = await _overdraftSettingsRepository.GetSettingsAsync();

            if (wallet.Currency == Currency.None)
            {
                throw new ArgumentException(Messages.Service.InvalidCurrency);
            }
            var createdWallet = _walletFactory.Map(wallet, overdraftSettings);
            createdWallet.OwnerId = userId;
            await _walletRepository.CreateWallet(createdWallet);
        }

        public async Task<WalletResponseDTO> GetWalletAsync(int id, string userId)
        {
            var wallet = await _walletRepository.GetWalletAsync(id);
            var isOwnerOrMember = wallet.OwnerId == userId || wallet.AppUserWallets.Any(uw => uw.Id == userId);

            if (!isOwnerOrMember)
            {
                throw new UnauthorizedAccessException(Messages.Unauthorized);
            }
            if(wallet == null)
            {
                throw new ArgumentException(Messages.Service.WalletNotFound);
            }

            var walletToReturn = new WalletResponseDTO
            {
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                Name = wallet.Name,
                WalletType = wallet.WalletType,
                Id = wallet.Id,
                AppUserWallets = wallet.AppUserWallets,
                OwnerId = userId,

            };
            return walletToReturn;
        }
        public async Task<List<AppUser>> GetWalletMembersAsync(int walletId)
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);

            if (wallet == null)
            {
                throw new ArgumentException("Wallet not found.");
            }

            return wallet.AppUserWallets.ToList(); // Assuming `AppUserWallets` is the collection of members in the wallet
        }
        public async Task<List<UserWallet>> GetUserWalletsAsync(string userId)
        {
            return await _walletRepository.GetUserWalletsAsync(userId);
        }
        public async Task<List<UserWallet>> GetWalletsForProcessingAsync()
        {
            return await _walletRepository.GetWalletsForProcessingAsync();
        }

        public async Task RemoveMemberFromJointWalletAsync(int walletId, string userId, string ownerId)
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException(Messages.Unauthorized);
            }
            var userWallet = wallet.AppUserWallets.SingleOrDefault(uw => uw.Id == userId);

            if (userWallet == null)
            {
                throw new InvalidOperationException(Messages.Service.UserNotMemberOfWallet);
            }
            var user = await _userManager.FindByIdAsync(userId);
            user.JointWallets.Remove(wallet);

            await _walletRepository.RemoveMemberFromJointWalletAsync(walletId, userWallet);
        }

        public async Task ToggleOverdraftAsync(int walletId, string userId)
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);

            if (wallet == null)
            {
                throw new ArgumentException(Messages.Service.WalletNotFound);
            }

            if (wallet.OwnerId != userId || wallet.WalletType != WalletType.Personal)
            {
                throw new InvalidOperationException(Messages.Service.OverdraftOperationNotAllowed);
            }

            wallet.IsOverdraftEnabled = !wallet.IsOverdraftEnabled;

            bool result = await _walletRepository.UpdateWalletAsync();

            if (!result)
            {
                throw new Exception(Messages.Service.FailedToUpdateWallet);
            }
        }

        public async Task UpdateWalletAsync()
        {
            await _walletRepository.UpdateWalletAsync();
        }
    }
}
