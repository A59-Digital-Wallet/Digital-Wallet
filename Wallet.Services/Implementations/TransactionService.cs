﻿using System;
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
using Wallet.Services.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Wallet.Common.Exceptions;
using Wallet.Services.Validation.TransactionValidation;
using Wallet.Data.Migrations;

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
        private readonly VerifyEmailService _verifyEmailService;
        private readonly IMemoryCache _transactionCache;
        private readonly IEmailSender _emailSender;
        private readonly ITransactionValidator _transactionValidator;
        public TransactionService(ITransactionRepository transactionRepository, 
            IWalletRepository walletRepository, 
            ICurrencyExchangeService currencyExchangeService, 
            ICardRepository cardRepository, 
            ITransactionFactory transactionFactory, 
            UserManager<AppUser> userManager,
            VerifyEmailService verifyEmailService, 
            IMemoryCache transactionCache,
            IEmailSender emailSender,
            ITransactionValidator transactionValidator )
            
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _currencyExchangeService = currencyExchangeService;
            _cardRepository = cardRepository;
            _transactionFactory = transactionFactory;
            this.userManager = userManager;
            _verifyEmailService = verifyEmailService;
            _transactionCache = transactionCache;
            _emailSender = emailSender;
            _transactionValidator = transactionValidator;
        }

        public async Task CreateTransactionAsync(TransactionRequestModel transactionRequest, string userId, string verificationCode = null)
        {
            var wallet = await ValidateWalletOwnershipAsync(transactionRequest.WalletId, userId);

            if (transactionRequest.TransactionType == TransactionType.Withdraw || transactionRequest.TransactionType == TransactionType.Transfer)
            {
                _transactionValidator.ValidateOverdraftAndBalance(wallet, transactionRequest.Amount);
            }

            var user = await userManager.FindByIdAsync(userId);

            bool isHighValue = _transactionValidator.IsHighValueTransaction(transactionRequest, wallet);

            if (isHighValue)
            {
                if (verificationCode == null)
                {
                    await HandleHighValueTransactionAsync(transactionRequest, user, wallet);
                    if(transactionRequest.TransactionType == TransactionType.Transfer)
                    {
                        
                        throw new VerificationRequiredException(transactionRequest.Token, transactionRequest.WalletId, transactionRequest.Amount, transactionRequest.Description, transactionRequest.RecepientWalletId);
                    }
                    throw new VerificationRequiredException(transactionRequest.Token);
                }
                else
                {
                    await VerifyHighValueTransactionAsync(transactionRequest, user, verificationCode);
                }
            }

            var transaction = await PrepareTransactionAsync(transactionRequest, wallet);

            await ProcessTransactionAsync(transactionRequest, transaction, wallet);

            await SaveTransactionAsync(wallet, transaction);

            // If a category is selected, associate the transaction with it
            if (transactionRequest.CategoryId.HasValue)
            {
                await AddTransactionToCategoryAsync(transaction.Id, transactionRequest.CategoryId.Value, userId);
            }
        }

        private async Task<UserWallet> ValidateWalletOwnershipAsync(int walletId, string userId)
        {
            var wallet = await _walletRepository.GetWalletAsync(walletId);
            if (wallet == null)
            {
                throw new ArgumentException("Wallet does not exist.");
            }
            if (wallet.OwnerId != userId && !wallet.AppUserWallets.Any(uw => uw.Id == userId))
            {
                throw new ArgumentException("Not your wallet!");
            }

            return wallet;
        }

        private async Task HandleHighValueTransactionAsync(TransactionRequestModel transactionRequest, AppUser user, UserWallet wallet)
        {
            var code = GenerateVerificationCode();
            await StoreVerificationCodeAsync(user, code);
            await SendVerificationEmailAsync(user, code);

            var transactionToken = GenerateTransactionToken();
            transactionRequest.Token = transactionToken;

            StorePendingTransaction(transactionRequest, wallet, user.Id, code);
        }

        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private async Task StoreVerificationCodeAsync(AppUser user, string code)
        {
            user.EmailConfirmationCode = code;
            user.EmailConfirmationCodeGeneratedAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }

        private async Task SendVerificationEmailAsync(AppUser user, string code)
        {
            var subject = "Your Transaction Verification Code";
            var message = $"Hello {user.UserName},\n\nYour verification code is: {code}\n\nPlease enter this code to complete your transaction.";
            await _emailSender.SendEmail(subject, user.Email, user.UserName, message);
        }

        private string GenerateTransactionToken()
        {
            return Guid.NewGuid().ToString();
        }

        private void StorePendingTransaction(TransactionRequestModel transactionRequest, UserWallet wallet, string userId, string code)
        {
            var pendingTransaction = new PendingTransaction
            {
                TransactionRequest = transactionRequest,
                Wallet = wallet,
                UserId = userId,
                VerificationCode = code
            };

            _transactionCache.Set(transactionRequest.Token, pendingTransaction, TimeSpan.FromMinutes(10));
        }

        private async Task VerifyHighValueTransactionAsync(TransactionRequestModel transactionRequest, AppUser user, string verificationCode)
        {
            if (user.EmailConfirmationCode != verificationCode ||
                !user.EmailConfirmationCodeGeneratedAt.HasValue ||
                (DateTime.UtcNow - user.EmailConfirmationCodeGeneratedAt.Value).TotalMinutes > 10)
            {
                throw new ArgumentException("Invalid or expired verification code.");
            }

            await ClearVerificationCode(user);

            if (!_transactionCache.TryGetValue(transactionRequest.Token, out PendingTransaction pendingTransaction))
            {
                throw new InvalidOperationException("Transaction token is invalid or has expired.");
            }

            _transactionCache.Remove(transactionRequest.Token);
        }

        private async Task ClearVerificationCode(AppUser user)
        {
            user.EmailConfirmationCode = null;
            user.EmailConfirmationCodeGeneratedAt = null;
            await userManager.UpdateAsync(user);
        }

        private async Task<Transaction> PrepareTransactionAsync(TransactionRequestModel transactionRequest, UserWallet wallet)
        {
            var transaction = _transactionFactory.Map(transactionRequest);

            if (transactionRequest.IsRecurring)
            {
                transaction.IsRecurring = true;
                transaction.Interval = transactionRequest.RecurrenceInterval;
                transaction.NextExecutionDate = DateTime.UtcNow.AddInterval(transactionRequest.RecurrenceInterval);
                transaction.IsActive = true;
            }

            if (transactionRequest.CardId != 0)
            {
                var card = await _cardRepository.GetCardAsync(transactionRequest.CardId);
                if (card == null)
                {
                    throw new ArgumentException("Card does not exist.");
                }
                transaction.CardId = card.Id;
            }

            return transaction;
        }

        private async Task ProcessTransactionAsync(TransactionRequestModel transactionRequest, Transaction transaction, UserWallet wallet)
        {
            switch (transactionRequest.TransactionType)
            {
                case TransactionType.Transfer:
                    await HandleTransferAsync(transactionRequest, transaction, wallet);
                    break;

                case TransactionType.Withdraw:
                    wallet.Balance -= transactionRequest.Amount;
                    transaction.OriginalAmount = transactionRequest.Amount;
                    transaction.OriginalCurrency = wallet.Currency;
                    transaction.SentCurrency = wallet.Currency;
                    break;

                case TransactionType.Deposit:
                    wallet.Balance += transactionRequest.Amount;
                    transaction.OriginalAmount = transactionRequest.Amount;
                    transaction.SentCurrency = wallet.Currency;
                    transaction.OriginalCurrency= wallet.Currency;
                    break;
            }
        }

        private async Task SaveTransactionAsync(UserWallet wallet, Transaction transaction)
        {
            try
            {
                await _walletRepository.UpdateWalletAsync();
                await _transactionRepository.CreateTransactionAsync(transaction);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Transaction conflict detected. Please try again.");
            }
        }


        public async Task<bool> VerifyTransactionAsync(string transactionToken, string verificationCode)
        {
            // Retrieve the pending transaction using the token
            if (!_transactionCache.TryGetValue(transactionToken, out PendingTransaction pendingTransaction))
            {
                throw new InvalidOperationException("Transaction token is invalid or has expired.");
            }

            var user = await this.userManager.FindByIdAsync(pendingTransaction.UserId);

            // Verify the code using the stored values in the user entity
            if (user.EmailConfirmationCode != verificationCode ||
                !user.EmailConfirmationCodeGeneratedAt.HasValue ||
                (DateTime.UtcNow - user.EmailConfirmationCodeGeneratedAt.Value).TotalMinutes > 10)
            {
                return false; // Verification failed
            }

         
            // Proceed with creating the transaction
            await CreateTransactionAsync(pendingTransaction.TransactionRequest, pendingTransaction.UserId, verificationCode);

            // Return success after transaction completion
            return true; // Transaction successfully completed
        }



        private async Task HandleTransferAsync(TransactionRequestModel transactionRequest, Transaction transaction, UserWallet wallet)
        {
            
            
            var recipientWallet = await _walletRepository.GetWalletAsync((int)transactionRequest.RecepientWalletId);

            if (recipientWallet == null)
            {
                throw new ArgumentException("Recipient wallet does not exist.");
            }
            transaction.OriginalAmount = transactionRequest.Amount;
            transaction.OriginalCurrency = wallet.Currency;
            transaction.SentCurrency = recipientWallet.Currency;

            if (wallet.Currency != recipientWallet.Currency)
            {
                transaction.Amount = await _currencyExchangeService.ConvertAsync(transactionRequest.Amount, wallet.Currency, recipientWallet.Currency);
            }

            wallet.Balance -= transactionRequest.Amount;
            recipientWallet.Balance += transaction.Amount;

            // Save the recipient wallet in the transaction
            transaction.RecipientWalletId = recipientWallet.Id;
            
           
                bool response = await _walletRepository.UpdateWalletAsync();
           
                if (!response)
                {
                    throw new InvalidOperationException();
                }
            
            
                // Handle concurrency exception, e.g., by retrying or logging the issue
            

            
        }

        public async Task<ICollection<TransactionDto>> FilterTransactionsAsync(int page, int pageSize, TransactionRequestFilter filterParameters, string userId)
        {
            var transactions = await _transactionRepository.FilterBy(page, pageSize, filterParameters, userId);
            var userWalletIds = (await _walletRepository.GetUserWalletsAsync(userId)).Select(w => w.Id).ToList();

            return transactions.Select(t =>
            {
                var transactionDto = _transactionFactory.Map(t);
                // Determine direction for each transaction based on user's wallet involvement
                transactionDto.Direction = userWalletIds.Contains(t.WalletId)
                    ? DetermineDirection(transactionDto, t.WalletId)
                    : DetermineDirection(transactionDto, t.RecipientWalletId);
                return transactionDto;
            }).ToList();
        }


        public async Task<UserWithWalletsDto> SearchUserWithWalletsAsync(string searchTerm)
        {
            var user = await this.userManager.Users
                .Include(u => u.OwnedWallets)
                .Include(u => u.JointWallets)
                .Include(u => u.Categories)
                .Include(u => u.LastSelectedWallet)
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
                        
                    }).ToList(),
                    Categories = u.Categories.ToList(),
                    PreferredWallet = u.LastSelectedWallet
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return user;
        }

        public async Task ProcessRecurringTransactionsAsync()
        {
            var now = DateTime.UtcNow;
            var recurringTransactions = await _transactionRepository.GetRecurringTransactionsDueAsync(now);

            foreach (var transaction in recurringTransactions)
            {
                try
                {
                    var wallet = await _walletRepository.GetWalletAsync(transaction.WalletId);
                    if (wallet.Balance < transaction.Amount)
                    {
                        // Notify user about insufficient funds and deactivate recurring transaction
                        transaction.IsActive = false;
                        await _transactionRepository.UpdateTransactionAsync(transaction);
                        // Notify user logic here (email, push notification, etc.)
                        continue;
                    }

                    if (transaction.RecipientWalletId.HasValue)
                    {
                        var recipientWallet = await _walletRepository.GetWalletAsync(transaction.RecipientWalletId.Value);
                        recipientWallet.Balance += transaction.Amount;
                        await _walletRepository.UpdateWalletAsync();
                    }
                    if(transaction.TransactionType == TransactionType.Withdraw || transaction.TransactionType == TransactionType.Transfer) 
                    {
                        wallet.Balance -= transaction.Amount;
                    }
                    else
                    {
                        wallet.Balance += transaction.Amount;
                    }
                    
                    await _walletRepository.UpdateWalletAsync();

                    transaction.LastExecutedDate = now;
                    transaction.NextExecutionDate = now.AddInterval(transaction.Interval);
                    await _transactionRepository.UpdateTransactionAsync(transaction);

                    var newTransaction = new Transaction
                    {
                        WalletId = transaction.WalletId,
                        Amount = transaction.Amount,
                        Description = transaction.Description,
                        TransactionType = transaction.TransactionType,
                        Date = now,
                        RecipientWalletId = transaction.RecipientWalletId,
                        IsRecurring = true,
                        Interval = transaction.Interval,
                        OriginalAmount = transaction.OriginalAmount,
                        OriginalCurrency = transaction.OriginalCurrency,
                        SentCurrency = transaction.SentCurrency,
                    };
                    transaction.IsRecurring = false;
                    transaction.Interval = default;
                    await _transactionRepository.CreateTransactionAsync(newTransaction);

                }
                catch (Exception ex)
                {
                    // Log the error and handle exceptions
                    // You might want to deactivate the transaction or retry later
                }
            }
        }

        public async Task CancelRecurringTransactionAsync(int transactionId, string userId)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);

            if (transaction == null)
            {
                throw new ArgumentException("Transaction not found.");
            }

            if (transaction.Wallet.OwnerId != userId && !transaction.Wallet.AppUserWallets.Any(uw => uw.Id == userId))
            {
                throw new UnauthorizedAccessException("You do not have permission to cancel this transaction.");
            }

            if (!transaction.IsRecurring)
            {
                throw new InvalidOperationException("This transaction is not a recurring transaction.");
            }

            transaction.IsRecurring = false;
            transaction.IsActive = false;
            transaction.NextExecutionDate = null;

            await _transactionRepository.UpdateTransactionAsync(transaction);
        }

        public async Task AddTransactionToCategoryAsync(int transactionId, int categoryId, string userId)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new EntityNotFoundException("Transaction not found.");
            }

            if (transaction.Wallet.OwnerId != userId && !transaction.Wallet.AppUserWallets.Any(uw => uw.Id == userId))
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this transaction.");
            }

            transaction.CategoryId = categoryId;

            await _transactionRepository.UpdateTransactionAsync(transaction);
        }

        private string DetermineDirection(TransactionDto transaction, int? walletId)
        {
            // Incoming if it's a deposit or a transfer to this wallet
            if (transaction.TransactionType == TransactionType.Deposit ||
                (transaction.TransactionType == TransactionType.Transfer && transaction.RecepientWalledId == walletId))
            {
                return "Incoming";
            }

            // Outgoing if it's a withdrawal or a transfer from this wallet
            return "Outgoing";
        }
        private string DetermineDirection(Transaction transaction, int walletId)
        {
            // Determine direction based on transaction type or recipient wallet ID
            if (transaction.TransactionType == TransactionType.Deposit ||
                (transaction.TransactionType == TransactionType.Transfer && transaction.RecipientWalletId == walletId))
            {
                return "Incoming";
            }
            return "Outgoing";
        }

        public async Task<(List<string> WeekLabels, List<decimal> WeeklySpendingAmounts)> GetWeeklySpendingAsync(int walletId)
        {
            var transactions = await _transactionRepository.GetTransactionsByWalletId(walletId);

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var weeks = new List<string>();
            var weeklySpending = new List<decimal>();

            for (int i = 0; i < 5; i++) // Assuming a maximum of 5 weeks in a month
            {
                var startOfWeek = startOfMonth.AddDays(i * 7);
                var endOfWeek = startOfWeek.AddDays(6);
                var weekLabel = $"{startOfWeek:MMM dd} - {endOfWeek:MMM dd}";

                weeks.Add(weekLabel);

                var totalForWeek = transactions
                    .Where(t => DetermineDirection(t, walletId) == "Outgoing" && t.Date >= startOfWeek && t.Date <= endOfWeek)
                    .Sum(t => t.OriginalAmount); // Use OriginalAmount instead of Amount

                weeklySpending.Add(totalForWeek);
            }

            return (weeks, weeklySpending);
        }

        public async Task<Dictionary<string, decimal>> GetMonthlySpendingByCategoryAsync(string userId, int walletId)
        {
            var transactions = await _transactionRepository.GetTransactionsByWalletId(walletId);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var spendingByCategory = transactions
                .Where(t => t.TransactionType == TransactionType.Withdraw || t.TransactionType == TransactionType.Transfer)
                .Where(t => t.Date.Month == currentMonth && t.Date.Year == currentYear)
                .Where(t => t.Category != null) // Ensure that the transaction has a category
                .GroupBy(t => t.Category.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(t => t.Amount)
                );

            return spendingByCategory;
        }



        public async Task<IEnumerable<Transaction>> GetTransactionHistoryContactAsync(string userId, string contactId)
        {
            // Get all wallet IDs for both the user and the contact
            var userWalletIds = (await _walletRepository.GetUserWalletsAsync(userId)).Select(w => w.Id).ToList();
            var contactWalletIds = (await _walletRepository.GetUserWalletsAsync(contactId)).Select(w => w.Id).ToList();
            var transactions = await _transactionRepository.GetTransactionHistoryContactAsync(userWalletIds, contactWalletIds);

               
             

            return transactions;
        }


    }
}
