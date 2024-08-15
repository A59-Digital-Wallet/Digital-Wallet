using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Wallet.Data.Db
{
    public class ApplicationContext : IdentityDbContext<AppUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<Card> Cards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UserWallet> Wallets { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set up relationship between AppUser and Cards with DeleteBehavior.Restrict
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Cards)
                .WithOne(c => c.AppUser)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Set up relationship between AppUser and Wallets where the user is the owner with DeleteBehavior.Restrict
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.OwnedWallets)
                .WithOne(w => w.Owner)
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure Wallet name is unique per AppUser
            modelBuilder.Entity<UserWallet>()
                .HasIndex(w => new { w.OwnerId, w.Name })
                .IsUnique();

            // Set up relationship between UserWallet and Transactions with DeleteBehavior.Cascade
            modelBuilder.Entity<UserWallet>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // Set up many-to-many relationship between UserWallet and AppUser (Associated Users)
            modelBuilder.Entity<UserWallet>()
                .HasMany(w => w.AppUserWallets)
                .WithMany(u => u.JointWallets)
                .UsingEntity<Dictionary<string, object>>(
                    "UserWalletAssociations",
                    j => j.HasOne<AppUser>().WithMany().HasForeignKey("OwnerId").OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<UserWallet>().WithMany().HasForeignKey("UserWalletId").OnDelete(DeleteBehavior.Restrict)
                );

            // Set up relationship between Card and Transactions with DeleteBehavior.SetNull
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Card)
                .WithMany()  // No navigation property on the Card side
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.SetNull);

            // Set up one-to-many relationship between User and Contacts
            modelBuilder.Entity<Contact>()
                 .HasOne(c => c.User)
                 .WithMany(u => u.Contacts)
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

            // One contact user can be in many contact lists
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.ContactUser)
                .WithMany()
                .HasForeignKey(c => c.ContactUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one - to - many relationship between AppUser and Category
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Categories)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, categories will be removed too

            // Configure one-to-many relationship between Category and Transaction
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Transactions)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // Set to null instead of deleting transactions

            modelBuilder.Entity<Transaction>()
                .Property(t => t.CardId)
                .IsRequired(false);

            // Configure PhoneNumber length for AppUser
            modelBuilder.Entity<AppUser>()
                .Property(u => u.PhoneNumber)
                .HasMaxLength(15);

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

            // Configure enum to string conversion for UserWallet Currency
            modelBuilder.Entity<UserWallet>()
                .Property(w => w.Currency)
                .HasConversion<string>();

            // Configure decimal properties
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UserWallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<UserWallet>()
                .Property(w => w.InterestRate)
                .HasColumnType("decimal(18,4)"); 

            modelBuilder.Entity<UserWallet>()
                .Property(w => w.OverdraftLimit)
                .HasColumnType("decimal(18,2)"); 

            modelBuilder.Entity<Transaction>()
              .Property(t => t.IsRecurring)
              .HasDefaultValue(false);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Interval)
                .HasConversion<string>();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.NextExecutionDate)
                .IsRequired(false);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.IsActive)
                .HasDefaultValue(true);

        }
    }
}
