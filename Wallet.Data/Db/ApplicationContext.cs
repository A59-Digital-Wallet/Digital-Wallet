﻿using Microsoft.AspNetCore.Identity;
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
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UserWallet> Wallets { get; set; }

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
        }
    }
}
