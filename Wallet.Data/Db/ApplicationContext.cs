using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;

namespace Wallet.Data.Db
{
    public class ApplicationContext : IdentityDbContext<AppUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<Card> Cards { get; set; }

        public DbSet<Transfer> TransferMoneyTransactions { get; set; }
        public DbSet<NonTransfer> NonTransferTransactions { get; set; }
        public DbSet<UserWallet> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Cards)
                .WithOne(c => c.AppUser)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Wallets)
                .WithOne(w => w.AppUser)
                .HasForeignKey(w => w.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserWallet>()
                .HasIndex(w => new { w.AppUserId, w.Name })
                .IsUnique();

            modelBuilder.Entity<UserWallet>()
                .HasMany(w => w.NonTransferTransactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserWallet>()
                .HasMany(w => w.TransferMoneyTransactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.NoAction);



            // Configure inheritance for transactions
            modelBuilder.Entity<Transaction>()
                .HasDiscriminator<TransactionType>("TransactionType")
                .HasValue<NonTransfer>(TransactionType.Add)
                .HasValue<Transfer>(TransactionType.Transfer)
                .HasValue<NonTransfer>(TransactionType.Withdraw);

            // Configure enum to string conversion for TransactionStatus
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion<string>();

            // Configure enum to string conversion for CardType
            modelBuilder.Entity<Card>()
                .Property(c => c.CardType)
                .HasConversion<string>();

            // Configure enum to string conversion for CardNetwork
            modelBuilder.Entity<Card>()
                .Property(c => c.CardNetwork)
                .HasConversion<string>();

            modelBuilder.Entity<NonTransfer>()
                .HasOne(t => t.UserCard)
                .WithMany()
                .HasForeignKey(t => t.UserCardID)
                .OnDelete(DeleteBehavior.NoAction);



            // Configure decimal properties
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UserWallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,2)");
        }
    }
}
