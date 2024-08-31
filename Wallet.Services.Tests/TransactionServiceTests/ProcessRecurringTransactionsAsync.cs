using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Implementations;
using Wallet.Data.Models.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Wallet.Data.Db;
using Wallet.Data.Repositories.Implementations;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Validation.TransactionValidation;

namespace Wallet.Services.Tests.TransactionServiceTests
{
    [TestClass]
    public class ProcessRecurringTransactionsAsyncTests
    {
        private TransactionService _transactionService;
        private DbContextOptions<ApplicationContext> _options;

        [TestInitialize]
        public void Setup()
        {
            // Setup in-memory database
            _options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ApplicationContext(_options))
            {
                // Seed the database with test data
                context.Wallets.Add(new UserWallet
                {
                    Id = 1,
                    Name = "Main Wallet", // Setting the required 'Name' property
                    OwnerId = "user1",
                    Balance = 1000,
                    IsOverdraftEnabled = false,
                    WalletType = WalletType.Personal,
                    Currency = Currency.USD // Ensure other necessary properties are set
                });

                context.Transactions.Add(new Transaction
                {
                    Id = 1,
                    WalletId = 1,
                    Amount = 100,
                    TransactionType = TransactionType.Withdraw,
                    IsRecurring = true,
                    NextExecutionDate = DateTime.UtcNow.AddDays(1),
                    OriginalAmount = 100,
                    OriginalCurrency = Currency.USD,
                    SentCurrency = Currency.USD,
                    Description = "Mopney for friend",
                    LastExecutedDate = DateTime.UtcNow,
                });

                context.SaveChanges();
            }

            var mockUserManager = new Mock<UserManager<AppUser>>(Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
            var mockMemoryCache = new Mock<IMemoryCache>();
            var mockCurrencyExchangeService = new Mock<ICurrencyExchangeService>();
            var mockEmailSender = new Mock<IEmailSender>();
            var mockTransactionFactory = new Mock<ITransactionFactory>();
            var mockTransactionValidator = new Mock<ITransactionValidator>();

            // Use the actual database context
            var contextReal = new ApplicationContext(_options);
            var transactionRepository = new TransactionRepository(contextReal);
            var walletRepository = new WalletRepository(contextReal);

            _transactionService = new TransactionService(
                transactionRepository,
                walletRepository,
                mockCurrencyExchangeService.Object,
                null,
                mockTransactionFactory.Object,
                mockUserManager.Object,
                null,
                mockMemoryCache.Object,
                mockEmailSender.Object,
                mockTransactionValidator.Object
            );
        }


        [TestMethod]
        public async Task ProcessRecurringTransactionsAsync_Should_Process_Transactions()
        {
            // Act
            await _transactionService.ProcessRecurringTransactionsAsync();

            // Assert
            using (var context = new ApplicationContext(_options))
            {
                var transaction = context.Transactions.FirstOrDefault(t => t.Id == 1);
                Assert.IsNotNull(transaction);
                Assert.AreEqual(true, transaction.IsRecurring);
                Assert.IsNotNull(transaction.LastExecutedDate);
            }
        }
    }



}
