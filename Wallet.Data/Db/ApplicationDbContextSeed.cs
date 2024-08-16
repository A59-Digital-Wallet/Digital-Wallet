using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.Data.Db
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationContext>();

            // Seed default users
            await SeedUsersAsync(userManager);
            // Seed overdraft settings
            await SeedOverdraftSettingsAsync(context);
        }

        private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            // Seed admin user
            var defaultUser = new AppUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                PhoneNumber = "1234567890",
                ProfilePictureURL = "https://res.cloudinary.com/dpfnd2zns/image/upload/v1720789473/fphw7iwjnfj28hexdgtz.png" // Optional
            };

            if (await userManager.FindByEmailAsync(defaultUser.Email) == null)
            {
                await userManager.CreateAsync(defaultUser, "Admin@123"); // Password will be hashed internally
                await userManager.AddToRoleAsync(defaultUser, "Admin");
            }

            // Seed regular user
            var regularUser = new AppUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                FirstName = "Regular",
                LastName = "User",
                EmailConfirmed = true,
                PhoneNumber = "0987654321",
                ProfilePictureURL = "https://res.cloudinary.com/dpfnd2zns/image/upload/v1720789473/fphw7iwjnfj28hexdgtz.png" // Optional
            };

            if (await userManager.FindByEmailAsync(regularUser.Email) == null)
            {
                await userManager.CreateAsync(regularUser, "User@123"); // Password will be hashed internally
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }

        private static async Task SeedOverdraftSettingsAsync(ApplicationContext context)
        {
            if (!context.OverdraftSettings.Any())
            {
                var defaultSettings = new OverdraftSettings
                {
                    DefaultInterestRate = 0.05m,
                    DefaultOverdraftLimit = 500m,
                    DefaultConsecutiveNegativeMonths = 3
                };

                context.OverdraftSettings.Add(defaultSettings);
                await context.SaveChangesAsync();
            }
        }
    }
}
