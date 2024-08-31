using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Services.Tests
{
    public class MockWalletRepository
    {
        private List<UserWallet> sampleWallets;

        public Mock<IWalletRepository> GetMockRepository()
        {
            var mockRepository = new Mock<IWalletRepository>();

            // Sample data for testing
            sampleWallets = new List<UserWallet>
            {
                new UserWallet
                {
                    Id = 1,
                    Name = "Savings Wallet",
                    WalletType = WalletType.Savings,
                    Balance = 1000m,
                    Currency = Currency.USD,
                    IsOverdraftEnabled = false,
                    OwnerId = "user1",
                    AppUserWallets = new List<AppUser> { new AppUser { Id = "user1", UserName = "User1" } }
                },
                new UserWallet
                {
                    Id = 2,
                    Name = "Joint Wallet",
                    WalletType = WalletType.Joint,
                    Balance = 500m,
                    Currency = Currency.USD,
                    IsOverdraftEnabled = true,
                    OwnerId = "user1",
                    AppUserWallets = new List<AppUser> { new AppUser { Id = "user2", UserName = "User2" } }
                },
                 new UserWallet
                {
                    Id = 3,
                    Name = "Main Wallet",
                    WalletType = WalletType.Personal,
                    Balance = 500m,
                    Currency = Currency.USD,
                    IsOverdraftEnabled = true,
                    OwnerId = "user2",
                    AppUserWallets = new List<AppUser> { new AppUser { Id = "user2", UserName = "User2" } }
                }
            };

            // Mock GetSavingsWalletsAsync
            mockRepository.Setup(x => x.GetSavingsWalletsAsync())
                .ReturnsAsync(() => sampleWallets.Where(w => w.WalletType == WalletType.Savings).ToList());

            // Mock GetWalletsForProcessingAsync
            mockRepository.Setup(x => x.GetWalletsForProcessingAsync())
                .ReturnsAsync(() => sampleWallets.Where(w => w.IsOverdraftEnabled && w.Balance < 0).ToList());

            // Mock GetUserWalletsAsync
            mockRepository.Setup(x => x.GetUserWalletsAsync(It.IsAny<string>()))
                .ReturnsAsync((string userId) => sampleWallets.Where(w => w.OwnerId == userId || w.AppUserWallets.Any(uw => uw.Id == userId)).ToList());

            // Mock AddMemberToJointWalletAsync
            mockRepository.Setup(x => x.AddMemberToJointWalletAsync(It.IsAny<int>(), It.IsAny<AppUser>()))
                .Callback((int walletId, AppUser userWallet) =>
                {
                    var wallet = sampleWallets.FirstOrDefault(w => w.Id == walletId);
                    if (wallet != null)
                    {
                        wallet.AppUserWallets.Add(userWallet);
                    }
                });

            // Mock CreateWallet
            mockRepository.Setup(x => x.CreateWallet(It.IsAny<UserWallet>()))
                .Callback((UserWallet wallet) =>
                {
                    wallet.Id = sampleWallets.Max(w => w.Id) + 1; // Assign a new ID
                    sampleWallets.Add(wallet);
                });

            // Mock GetWalletAsync
            mockRepository.Setup(x => x.GetWalletAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => sampleWallets.FirstOrDefault(w => w.Id == id));

            // Mock RemoveMemberFromJointWalletAsync
            mockRepository.Setup(x => x.RemoveMemberFromJointWalletAsync(It.IsAny<int>(), It.IsAny<AppUser>()))
                .Callback((int walletId, AppUser userWallet) =>
                {
                    var wallet = sampleWallets.FirstOrDefault(w => w.Id == walletId);
                    if (wallet != null)
                    {
                        wallet.AppUserWallets.Remove(userWallet);
                    }
                });

            // Mock UpdateWalletAsync
            mockRepository.Setup(x => x.UpdateWalletAsync())
                .ReturnsAsync(true);

            return mockRepository;
        }
    }
}
