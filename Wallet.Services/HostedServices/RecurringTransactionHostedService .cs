using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wallet.Services.Contracts;

namespace Wallet.Services.HostedServices
{
    public class RecurringTransactionHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public RecurringTransactionHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ProcessRecurringTransactions, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private async void ProcessRecurringTransactions(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                await transactionService.ProcessRecurringTransactionsAsync();
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
