using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Data.Repositories.Implementations
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationContext _context;

        public TransactionRepository(ApplicationContext applicationContext)
        {
            this._context = applicationContext;
        }
        public async Task UpdateTransactionAsync(Transaction transaction) // Add this method
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync() ;
        }

        public async Task<ICollection<Transaction>> GetRecurringTransactionsDueAsync(DateTime dueDate)
        {
            return await _context.Transactions
                .Where(t => t.IsRecurring && t.IsActive && t.NextExecutionDate <= dueDate)
                .ToListAsync();
        }
             
        public async Task CreateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }
        public async Task<Transaction> GetTransactionByIdAsync(int transactionId)
        {
            return await _context.Transactions
                                 .Include(t => t.Wallet) 
                                 .ThenInclude(u => u.Owner)
                                 .Include(t => t.Wallet)
                                 .ThenInclude( u => u.AppUserWallets)
                                 .FirstOrDefaultAsync(t => t.Id == transactionId);
        }

        public async Task<IList<Transaction>> GetTransactionsByWalletId(int walletId)
        {
            return await _context.Transactions
                                 .Where(t => t.WalletId == walletId || t.RecipientWalletId == walletId)
                                 .Include(t => t.Wallet)
                                 .ThenInclude(u => u.Owner)
                                 .ToListAsync();
        }
        public async Task<IList<Transaction>> GetTransactionsByUserId(string userId)
        {
            return await _context.Transactions
                                 .Where(t => t.Wallet.OwnerId == userId || t.RecipientWallet.OwnerId== userId || t.Wallet.AppUserWallets.Any(x => x.Id == userId) || t.RecipientWallet.AppUserWallets.Any(x => x.Id == userId))
                                 .Include(t => t.Wallet) 
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionHistoryContactAsync(List<int> userWalletIds, List<int> contactWalletIds)
        {
            return await _context.Transactions
                .Where(t =>
                    (userWalletIds.Contains(t.WalletId) && contactWalletIds.Contains((int)t.RecipientWalletId)) ||
                    (userWalletIds.Contains((int)t.RecipientWalletId) && contactWalletIds.Contains(t.WalletId)))
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }
        public async Task<ICollection<Transaction>> FilterBy(int page, int pageSize, TransactionRequestFilter filterParameters, string userId)
        {
            IList<Transaction> results = await GetTransactionsByUserId(userId);

            // Apply filters
            results = FilterByDate(results, filterParameters.Date, filterParameters.StartDate, filterParameters.EndDate);
            results = FilterByTransactionType(results, filterParameters.TransactionType);
            results = FilterByWalletId(results, filterParameters.WalletId);
            results = FilterByCurrency(results, filterParameters.Currency);

            // Apply sorting
            results = SortBy(results, filterParameters.SortBy);

            // Convert to list and apply order if necessary
            var orderedResults = Order(results.ToList(), filterParameters.OrderBy);

            // Apply pagination
            return await GetPagedTransactions(page, pageSize, orderedResults);
        }

        private static IList<Transaction> FilterByDate(IList<Transaction> transactions, DateTime? date, DateTime? startDate, DateTime? endDate)
        {
            if (date.HasValue)
            {
                // Binary search for the specific day
                int startIndex = BinarySearch(transactions, date.Value, true);
                int endIndex = BinarySearch(transactions, date.Value, false);

                return transactions.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                // Binary search for the start and end of the range
                int startIndex = BinarySearch(transactions, startDate.Value, true);
                int endIndex = BinarySearch(transactions, endDate.Value, false);

                return transactions.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
            }

            if (startDate.HasValue)
            {
                // Binary search for the start of the range
                int startIndex = BinarySearch(transactions, startDate.Value, true);
                return transactions.Skip(startIndex).ToList();
            }

            if (endDate.HasValue)
            {
                // Binary search for the end of the range
                int endIndex = BinarySearch(transactions, endDate.Value, false);
                return transactions.Take(endIndex + 1).ToList();
            }

            return transactions;
        }
        private static IList<Transaction> FilterByTransactionType(IList<Transaction> transactions, TransactionType transactionType)
        {
            if (transactionType != TransactionType.None)
            {
                return transactions.Where(t => t.TransactionType == transactionType).ToList();
            }
            return transactions;
        }
        private static IList<Transaction> FilterByWalletId(IList<Transaction> transactions, int walletId)
        {
            if (walletId > 0)
            {
                return transactions.Where(t => t.WalletId == walletId || t.RecipientWalletId == walletId).ToList();
            }
            return transactions;
        }
        private static IList<Transaction> FilterByCurrency(IList<Transaction> transactions, Currency currency)
        {
            if (currency != Currency.None)
            {
                return transactions.Where(t => t.Wallet.Currency == currency).ToList();
            }
            return transactions;
        }
        private static IList<Transaction> SortBy(IList<Transaction> transactions, string sortCriteria)
        {
            switch (sortCriteria)
            {
                case "Amount":
                    return transactions.OrderBy(t => t.Amount).ToList();
                case "Status":
                    return transactions.OrderBy(t => t.Status).ToList();
                default:
                    return transactions; // Default to sorting by Date descending
            }
        }
        private static IList<Transaction> Order(IList<Transaction> transactions, string sortOrder)
        {
            return (sortOrder == "desc") ? transactions.Reverse().ToList() : transactions;
        }
        private static async Task<ICollection<Transaction>> GetPagedTransactions(int page, int pageSize, ICollection<Transaction> filteredTransactions)
        {
            int skip = (page - 1) * pageSize;
            return filteredTransactions
                .Skip(skip)
                .Take(pageSize)
                .ToList();
        }
        private static int BinarySearch(IList<Transaction> transactions, DateTime targetDate, bool searchStart)
        {
            int left = 0;
            int right = transactions.Count - 1;
            int result = -1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                var midDate = transactions[mid].Date.Date;

                if (midDate < targetDate.Date)
                {
                    left = mid + 1;
                }
                else if (midDate > targetDate.Date)
                {
                    right = mid - 1;
                }
                else
                {
                    result = mid;
                    if (searchStart)
                    {
                        right = mid - 1; // Narrow search to the left half
                    }
                    else
                    {
                        left = mid + 1; // Narrow search to the right half
                    }
                }
            }

            return result == -1 ? left : result;
        }

       
    }
}
