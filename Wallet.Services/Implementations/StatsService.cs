using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class StatsService : IStatsService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrencyExchangeService _currencyExchangeService;

        public StatsService(
            ITransactionRepository transactionRepository,
            IWalletRepository walletRepository,
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
            var transactions = await _transactionRepository.FilterBy(1, int.MaxValue, filter, userId);
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

                foreach (var transaction in transactions.Where(t => t.WalletId == wallet.Id || t.RecipientWalletId == wallet.Id))
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
                        case TransactionType.Transfer when transaction.RecipientWalletId == wallet.Id:
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

    }
}
