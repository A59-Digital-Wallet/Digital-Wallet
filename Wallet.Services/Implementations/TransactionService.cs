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
using Wallet.DTO.Response;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace Wallet.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly ICardRepository _cardRepository;
        private readonly ITransactionFactory _transactionFactory;
        private readonly UserManager<AppUser> userManager;
        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository, ICurrencyExchangeService currencyExchangeService, ICardRepository cardRepository, ITransactionFactory transactionFactory, UserManager<AppUser> userManager)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _currencyExchangeService = currencyExchangeService;
            _cardRepository = cardRepository;
            _transactionFactory = transactionFactory;
            this.userManager = userManager;
        }

        public async Task CreateTransactionAsync(TransactionRequestModel transactionRequest, string userId )
        {
            var wallet = await _walletRepository.GetWalletAsync(transactionRequest.WalletId);
            if (wallet.OwnerId != userId && !wallet.AppUserWallets.Any(uw => uw.Id == userId))
            {
                throw new ArgumentException("Not your wallet!");
            }
          
            else if (wallet == null)
            {
                throw new ArgumentException("Wallet does not exist.");
            }
            var user = await this.userManager.FindByIdAsync(userId);
           

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
                    if (await this.userManager.IsInRoleAsync(user, "Blocked"))
                    {
                        throw new InvalidOperationException("Blocked users are not allowed to transfer money to other users.");
                    }
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
            await _walletRepository.UpdateWalletAsync(wallet);
            await _walletRepository.UpdateWalletAsync(recipientWallet);
        }

        public async Task<ICollection<TransactionDto>> FilterTransactionsAsync(int page, int pageSize, TransactionRequestFilter filterParameters, string userId)
        {
            var transactions = await _transactionRepository.FilterBy(page, pageSize, filterParameters, userId);
            return transactions.Select(t => _transactionFactory.Map(t)).ToList();
        }

        public async Task<UserWithWalletsDto> SearchUserWithWalletsAsync(string searchTerm)
        {
            var user = await this.userManager.Users
                .Include(u => u.OwnedWallets)
                .Include(u => u.JointWallets)
                .Where(u => u.UserName.Contains(searchTerm) || u.Email.Contains(searchTerm) || u.PhoneNumber.Contains(searchTerm))
                .Select(u => new UserWithWalletsDto
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Wallets = u.OwnedWallets.Select(w => new WalletDto
                    {
                        WalletId = w.Id,
                        Currency = w.Currency,
                        Balance = w.Balance
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return user;
        }


    }

}
