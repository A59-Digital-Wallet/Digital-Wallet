using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Contracts;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Models.Enum;
using Wallet.DTO.Request;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly ICardRepository _cardRepository;
        private readonly ITransactionFactory _transactionFactory;

        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository, ICurrencyExchangeService currencyExchangeService, ICardRepository cardRepository, ITransactionFactory transactionFactory)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _currencyExchangeService = currencyExchangeService;
            _cardRepository = cardRepository;
            _transactionFactory = transactionFactory;
        }

        public async Task CreateTransactionAsync(TransactionRequestModel transactionRequest, string userId )
        {
            var wallet = await _walletRepository.GetWalletAsync(transactionRequest.WalletId);
            if(wallet.AppUserId != userId)
            {
                throw new ArgumentException("Not your wallet!");
            }
            if (wallet == null)
            {
                throw new ArgumentException("Wallet does not exist.");
            }

            Transaction transaction = _transactionFactory.Map(transactionRequest);

            if (transactionRequest.CardId != 0)
            {
                var card = await _cardRepository.GetCardAsync(transactionRequest.CardId);
                if (card == null)
                {
                    throw new ArgumentException("Card does not exist.");
                }
                transaction.CardId = card.Id;
            }

            
            switch (transactionRequest.TransactionType)
            {
                case TransactionType.Transfer:
                    await HandleTransferAsync(transactionRequest, transaction, wallet);
                    break;

                case TransactionType.Withdraw:
                    if (wallet.Balance < transactionRequest.Amount)
                    {
                        throw new InvalidOperationException("Insufficient funds.");
                    }
                    wallet.Balance -= transactionRequest.Amount;
                    break;

                case TransactionType.Deposit:
                    wallet.Balance += transactionRequest.Amount;
                    break;
            }

            
            await _walletRepository.UpdateWalletAsync(wallet);
            await _transactionRepository.CreateTransactionAsync(transaction);
        }

        private async Task HandleTransferAsync(TransactionRequestModel transactionRequest, Transaction transaction, UserWallet wallet)
        {
            var recipientWallet = await _walletRepository.GetWalletAsync(transactionRequest.RecepientWalletId);

            if (recipientWallet == null)
            {
                throw new ArgumentException("Recipient wallet does not exist.");
            }

            if (wallet.Currency != recipientWallet.Currency)
            {
                transaction.Amount = await _currencyExchangeService.ConvertAsync(transactionRequest.Amount, wallet.Currency, recipientWallet.Currency);
            }

            if (wallet.Balance < transactionRequest.Amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            wallet.Balance -= transactionRequest.Amount;
            recipientWallet.Balance += transaction.Amount;

            // Save the recipient wallet in the transaction
            transaction.RecipientWalletId = recipientWallet.Id;

            // Save recipient wallet changes
            await _walletRepository.UpdateWalletAsync(recipientWallet);
        }

        public async Task<ICollection<Transaction>> FilterTransactionsAsync(int page, int pageSize, TransactionRequestFilter filterParameters, string userID)
        {
            
            return await _transactionRepository.FilterBy(page, pageSize, filterParameters, userID);
        }

       

      
    }

}
