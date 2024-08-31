using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wallet.Data.Models.Enum;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Services.Tests
{
    public class MockTransactionRepository
    {
        private List<Transaction> _sampleTransactions;

        public Mock<ITransactionRepository> GetMockRepository()
        {
            var mockRepository = new Mock<ITransactionRepository>();

            _sampleTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 1,
                    WalletId = 1,
                    RecipientWalletId = 2,
                    Amount = 100,
                    Date = DateTime.UtcNow.AddDays(-10),
                    TransactionType = TransactionType.Deposit,
                    OriginalCurrency = Wallet.Data.Models.Enums.Currency.USD,
                    Status = TransactionStatus.Pending,
                },
                new Transaction
                {
                    Id = 2,
                    WalletId = 2,
                    RecipientWalletId = 3,
                    Amount = 50,
                    Date = DateTime.UtcNow.AddDays(-5),
                    TransactionType = TransactionType.Transfer,
                    OriginalCurrency = Wallet.Data.Models.Enums.Currency.USD,
                    Status = TransactionStatus.Pending,
                }
            };

            // Mock GetTransactionByIdAsync
            mockRepository.Setup(repo => repo.GetTransactionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => _sampleTransactions.FirstOrDefault(t => t.Id == id));

            // Mock CreateTransactionAsync
            mockRepository.Setup(repo => repo.CreateTransactionAsync(It.IsAny<Transaction>()))
                .Callback((Transaction transaction) =>
                {
                    transaction.Id = _sampleTransactions.Max(t => t.Id) + 1;
                    _sampleTransactions.Add(transaction);
                })
                .Returns(Task.CompletedTask);

            // Mock GetTransactionsByWalletId
            mockRepository.Setup(repo => repo.GetTransactionsByWalletId(It.IsAny<int>()))
                .ReturnsAsync((int walletId) => _sampleTransactions.Where(t => t.WalletId == walletId || t.RecipientWalletId == walletId).ToList());

            // Mock GetRecurringTransactionsDueAsync
            mockRepository.Setup(repo => repo.GetRecurringTransactionsDueAsync(It.IsAny<DateTime>()))
                .ReturnsAsync((DateTime dueDate) => _sampleTransactions.Where(t => t.IsRecurring && t.NextExecutionDate <= dueDate).ToList());

            // Mock UpdateTransactionAsync
            // Mock UpdateTransactionAsync
            mockRepository.Setup(repo => repo.UpdateTransactionAsync(It.IsAny<Transaction>()))
                .Callback((Transaction transaction) =>
                {
                    var existingTransaction = _sampleTransactions.FirstOrDefault(t => t.Id == transaction.Id);
                    if (existingTransaction != null)
                    {
                        _sampleTransactions.Remove(existingTransaction);
                        _sampleTransactions.Add(transaction);
                    }
                })
                .Returns(Task.CompletedTask);

            // Add more mocks as needed...

            return mockRepository;
        }
    }
}
