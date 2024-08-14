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
using Wallet.Services.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Wallet.Common.Exceptions;

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
        public TransactionService(ITransactionRepository transactionRepository, 
            IWalletRepository walletRepository, 
            ICurrencyExchangeService currencyExchangeService, 
            ICardRepository cardRepository, 
            ITransactionFactory transactionFactory, 
            UserManager<AppUser> userManager,
             VerifyEmailService verifyEmailService
            , 
            IMemoryCache transactionCache, IEmailSender emailSender)
            
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
        }

        public async Task CreateTransactionAsync(TransactionRequestModel transactionRequest, string userId, string verificationCode = null)
        {
            var wallet = await _walletRepository.GetWalletAsync(transactionRequest.WalletId);
            if (wallet == null)
            {
                throw new ArgumentException("Wallet does not exist.");
            }
            if (wallet.OwnerId != userId && !wallet.AppUserWallets.Any(uw => uw.Id == userId))
            {
                throw new ArgumentException("Not your wallet!");
            }

            var user = await this.userManager.FindByIdAsync(userId);
            bool isHighValue = transactionRequest.Amount >= wallet.Balance * 0.8m || transactionRequest.Amount > 20000 && transactionRequest.TransactionType != TransactionType.Deposit;

            if (isHighValue && verificationCode == null)
            {
                // Generate and send verification code via email
                var code = new Random().Next(100000, 999999).ToString();
                user.EmailConfirmationCode = code;
                user.EmailConfirmationCodeGeneratedAt = DateTime.UtcNow;

                await this.userManager.UpdateAsync(user);

                var subject = "Your Transaction Verification Code";
                var message = $"Hello {user.UserName},\n\nYour verification code is: {code}\n\nPlease enter this code to complete your transaction.";
                await _emailSender.SendEmail(subject, user.Email, user.UserName, message);

                // Create a unique token for this transaction
                var transactionToken = Guid.NewGuid().ToString();
                transactionRequest.Token = transactionToken;
                // Store the transaction details temporarily in cache
                var pendingTransaction = new PendingTransaction
                {
                    TransactionRequest = transactionRequest,
                    Wallet = wallet,
                    UserId = userId,
                    VerificationCode = code // Store the generated code
                };

                _transactionCache.Set(transactionToken, pendingTransaction, TimeSpan.FromMinutes(10)); // Token expires in 10 minutes

                // Throw an exception to indicate that verification is required
                throw new VerificationRequiredException(transactionToken);
            }

            if (isHighValue && verificationCode != null)
            {
                // Verify the code using the stored values in the user entity
               

                // Mark email as confirmed if this is part of your logic
               
                user.EmailConfirmationCode = null;
                user.EmailConfirmationCodeGeneratedAt = null;
                await this.userManager.UpdateAsync(user);

                // Proceed with the transaction
                if (!_transactionCache.TryGetValue(transactionRequest.Token, out PendingTransaction pendingTransaction))
                {
                    throw new InvalidOperationException("Transaction token is invalid or has expired.");
                }

                

                // Remove the transaction from cache after successful verification
                _transactionCache.Remove(transactionRequest.Token);
            }

            // Map and save the transaction
            Transaction transaction = _transactionFactory.Map(transactionRequest);

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

            switch (transactionRequest.TransactionType)
            {
                case TransactionType.Transfer:
                    if (wallet.WalletType == WalletType.Savings)
                    {
                        throw new InvalidOperationException("Can't transfer from savings wallet");
                    }
                    if (await this.userManager.IsInRoleAsync(wallet.Owner, "Blocked"))
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

            try
            {
                await _walletRepository.UpdateWalletAsync(wallet);
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

           
                bool response = await _walletRepository.UpdateWalletAsync(wallet);
                if (!response)
                {
                    throw new InvalidOperationException();
                }
            
            
                // Handle concurrency exception, e.g., by retrying or logging the issue
            

            
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
                        await _walletRepository.UpdateWalletAsync(recipientWallet);
                    }

                    wallet.Balance -= transaction.Amount;
                    await _walletRepository.UpdateWalletAsync(wallet);

                    transaction.LastExecutedDate = now;
                    transaction.NextExecutionDate = now.AddInterval(transaction.Interval);
                    await _transactionRepository.UpdateTransactionAsync(transaction);
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

            transaction.IsActive = false;
            transaction.NextExecutionDate = null;

            await _transactionRepository.UpdateTransactionAsync(transaction);
        }


    }

}
