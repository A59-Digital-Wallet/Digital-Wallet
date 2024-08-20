using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Services.Contracts;

namespace Wallet.Services.HostedServices
{
    public class UserBlockUnblockService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public UserBlockUnblockService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Run the service once a day at midnight
            _timer = new Timer(ProcessUsers, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void ProcessUsers(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var overdraftService = scope.ServiceProvider.GetRequiredService<IOverdraftSettingsService>();
                var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var overdraftSettings = await overdraftService.GetSettingsAsync();
                var wallets = await walletService.GetWalletsForProcessingAsync();

                foreach (var wallet in wallets)
                {
                    if (wallet.Balance < 0)
                    {
                        // Apply interest to the negative balance
                        var interestAmount = wallet.Balance * overdraftSettings.DefaultInterestRate;
                        wallet.Balance -= interestAmount; // Subtracting because the balance is negative

                        // Increment the consecutive negative months count
                        wallet.ConsecutiveNegativeMonths++;

                        // Check if the user should be blocked
                        if (wallet.ConsecutiveNegativeMonths >= overdraftSettings.DefaultConsecutiveNegativeMonths)
                        {
                            await userService.ManageRoleAsync(wallet.OwnerId, "block");
                        }

                        // Save changes to the wallet
                        await walletService.UpdateWalletAsync();
                    }
                    else if (wallet.Balance >= 0 && wallet.ConsecutiveNegativeMonths > 0)
                    {
                        // Unblock the user if the wallet balance is non-negative
                        await userService.ManageRoleAsync(wallet.OwnerId, "unblock");

                        // Reset the ConsecutiveNegativeMonths count after unblocking
                        wallet.ConsecutiveNegativeMonths = 0;
                        await walletService.UpdateWalletAsync();
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
