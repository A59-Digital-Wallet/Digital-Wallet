using Twilio.Rest.Events.V1.Subscription;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class StatsService : IStatsService
    {
        private readonly ITransactionService _transactionRepository;
        private readonly IWalletService _walletRepository;
        private readonly ICurrencyExchangeService _currencyExchangeService;

        public StatsService(
            ITransactionService transactionRepository,
            IWalletService walletRepository,
            ICurrencyExchangeService currencyExchangeService)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _currencyExchangeService = currencyExchangeService;
        }

        public async Task<StatsViewModel> GetUserStatsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            endDate ??= startDate.Value.AddMonths(1).AddDays(-1);

            var filter = new TransactionRequestFilter
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var wallets = await _walletRepository.GetUserWalletsAsync(userId);
            var transactions = await _transactionRepository.FilterTransactionsAsync(1, int.MaxValue, filter, userId);
            var baseCurrency = Currency.BGN;

            decimal totalBalanceInBGN = 0;
            var walletBreakdown = new List<WalletStatsViewModel>();

            foreach (var wallet in wallets)
            {
                decimal walletBalanceInBGN = wallet.Currency != baseCurrency
                    ? await _currencyExchangeService.ConvertAsync(wallet.Balance, wallet.Currency, baseCurrency)
                    : wallet.Balance;

                totalBalanceInBGN += walletBalanceInBGN;

                decimal totalDeposits = 0;
                decimal totalWithdrawals = 0;
                decimal totalTransfersSent = 0;
                decimal totalTransfersReceived = 0;

                foreach (var transaction in transactions.Where(t => t.WalletId == wallet.Id || t.RecepientWalledId == wallet.Id))
                {
                    var transactionAmount = transaction.Amount;

                    switch (transaction.TransactionType)
                    {
                        case TransactionType.Deposit when transaction.WalletId == wallet.Id:
                            totalDeposits += transactionAmount;
                            break;
                        case TransactionType.Withdraw when transaction.WalletId == wallet.Id:
                            totalWithdrawals += transactionAmount;
                            break;
                        case TransactionType.Transfer when transaction.WalletId == wallet.Id:
                            totalTransfersSent += transactionAmount;
                            break;
                        case TransactionType.Transfer when transaction.RecepientWalledId == wallet.Id:
                            totalTransfersReceived += transactionAmount;
                            break;
                    }
                }

                walletBreakdown.Add(new WalletStatsViewModel
                {
                    WalletName = wallet.Name,
                    Currency = wallet.Currency,
                    Balance = wallet.Balance,
                    TotalDeposits = totalDeposits,
                    TotalWithdrawals = totalWithdrawals,
                    TotalTransfersSent = totalTransfersSent,
                    TotalTransfersReceived = totalTransfersReceived
                });
            }

            return new StatsViewModel
            {
                TotalBalance = totalBalanceInBGN,
                CurrencyCulture = "bg-BG", // Assuming BGN is the base currency
                WalletBreakdown = walletBreakdown,
                StartDate = startDate.Value,
                EndDate = endDate.Value
            };
        }
        public async Task<(List<string>, List<decimal>)> GetBalanceOverTime(int walletId, string interval, string userId)
        {
            var now = DateTime.UtcNow;
            DateTime startDate;
            Func<DateTime, DateTime> stepFunc;
            Func<DateTime, string> labelFormat;

            // Set up the range and step functions based on the interval
            switch (interval.ToLower())
            {
                case "daily":
                    startDate = now.AddDays(-14).Date;
                    stepFunc = date => date.AddDays(1);
                    labelFormat = date => date.ToString("yyyy-MM-dd");
                    break;
                case "weekly":
                    startDate = now.AddMonths(-3).Date;
                    startDate = startDate.AddDays(-(int)startDate.DayOfWeek); // Start at the beginning of the week
                    stepFunc = date => date.AddDays(7);
                    labelFormat = date => date.ToString("yyyy-MM-dd");
                    break;
                case "monthly":
                    startDate = now.AddMonths(-12);
                    startDate = new DateTime(startDate.Year, startDate.Month, 1); // Start at the beginning of the month
                    stepFunc = date => date.AddMonths(1);
                    labelFormat = date => date.ToString("yyyy-MM");
                    break;
                case "yearly":
                default:
                    startDate = now.AddYears(-5);
                    startDate = new DateTime(startDate.Year, 1, 1); // Start at the beginning of the year
                    stepFunc = date => date.AddYears(1);
                    labelFormat = date => date.ToString("yyyy");
                    break;
            }

            var filter = new TransactionRequestFilter
            {
                StartDate = startDate,
                EndDate = now,
                WalletId = walletId
            };

            var transactions = (await _transactionRepository.FilterTransactionsAsync(1, int.MaxValue, filter, userId)).ToList();

            var balanceLabels = new List<string>();
            var balanceAmounts = new List<decimal>();
            decimal runningBalance = 0m; // Start balance

            var currentDate = startDate;

            while (currentDate <= now.Date)
            {
                // Process all transactions that occur in the current interval
                foreach (var transaction in transactions.Where(t => t.Date.Date < stepFunc(currentDate)).ToList())
                {
                    bool isIncomingTransfer = walletId == transaction.RecepientWalledId;
                    runningBalance = ApplyTransactionToBalance(runningBalance, transaction, isIncomingTransfer);
                    transactions.Remove(transaction);  // Remove transaction after processing
                }

                // Add the balance for this interval
                balanceLabels.Add(labelFormat(currentDate));
                balanceAmounts.Add(runningBalance);

                // Move to the next interval
                currentDate = stepFunc(currentDate);
            }

            // Scan one more interval but do not add it to the labels
            foreach (var transaction in transactions.Where(t => t.Date.Date < stepFunc(currentDate)).ToList())
            {
                bool isIncomingTransfer = walletId == transaction.RecepientWalledId;
                runningBalance = ApplyTransactionToBalance(runningBalance, transaction, isIncomingTransfer);
            }

            return (balanceLabels, balanceAmounts);
        }

        private decimal ApplyTransactionToBalance(decimal balance, TransactionDto transaction, bool isIncomingTransfer)
        {
            switch (transaction.TransactionType)
            {
                case TransactionType.Deposit:
                    return balance + transaction.OriginalAmount;
                case TransactionType.Withdraw:
                    return balance - transaction.OriginalAmount;
                case TransactionType.Transfer:
                    return isIncomingTransfer ? balance + transaction.Amount : balance - transaction.OriginalAmount;
                default:
                    return balance;
            }
        }


    }
}
