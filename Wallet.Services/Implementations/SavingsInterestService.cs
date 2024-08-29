using System.Runtime.CompilerServices;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class SavingsInterestService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionService _transactionService;
        public SavingsInterestService(IWalletRepository walletRepository, ITransactionService transactionService)
        {
            _walletRepository = walletRepository;
            _transactionService = transactionService;
        }

        public async Task ApplyMonthlyInterestAsync()
        {
            var savingsWallets = await _walletRepository.GetSavingsWalletsAsync();

            foreach (var wallet in savingsWallets)
            {
                var interestAmount = wallet.Balance * 0.046m / 12;
                wallet.Balance += interestAmount; // 4.6% annual interest, applied monthly
                await _walletRepository.UpdateWalletAsync();

                var transactionRequest = new TransactionRequestModel
                {
                    WalletId = wallet.Id,
                    Amount = interestAmount,
                    Description = "Monthly Interest",
                    TransactionType = TransactionType.Deposit,
                    CardId = 5
                    
                };

                await _transactionService.CreateTransactionAsync(transactionRequest, wallet.OwnerId);
            }
        }
    }

}
