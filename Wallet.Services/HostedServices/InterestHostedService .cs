using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Implementations;

namespace Wallet.Services.HostedServices
{
    public class InterestHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public InterestHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Schedule the task to run monthly (or as needed)
            _timer = new Timer(ApplyInterest, null, TimeSpan.Zero, TimeSpan.FromDays(30));
            return Task.CompletedTask;
        }

        private void ApplyInterest(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var savingsInterestService = scope.ServiceProvider.GetRequiredService<SavingsInterestService>();
                savingsInterestService.ApplyMonthlyInterestAsync().Wait(); // Run your interest application logic here
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