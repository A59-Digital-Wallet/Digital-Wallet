using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;

namespace Wallet.Data.Db
{
    public class ApplicationContext : IdentityDbContext<AppUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<AddMoney> AddMoneyTransactions { get; set; }
        public DbSet<Transfer> TransferMoneyTransactions { get; set; }
        public DbSet<Withdraw> WithdrawMoneyTransactions { get; set; }
        public DbSet<Wallettt> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Cards)
                .WithOne(c => c.AppUser)
                .HasForeignKey(c => c.AppUserId);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Wallets)
                .WithOne(w => w.AppUser)
                .HasForeignKey(w => w.AppUserId);

            modelBuilder.Entity<Wallettt>()
                .HasIndex(w => new { w.AppUserId, w.Name })
                .IsUnique();

            modelBuilder.Entity<Wallettt>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId);

            // Configure inheritance for transactions
            modelBuilder.Entity<Transaction>()
                .HasDiscriminator<string>("TransactionType")
                .HasValue<AddMoney>("AddMoney")
                .HasValue<Transfer>("TransferMoney")
                .HasValue<Withdraw>("WithdrawMoney");

            // Configure enum to string conversion for TransactionStatus
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion<string>();
        }
    }
}
