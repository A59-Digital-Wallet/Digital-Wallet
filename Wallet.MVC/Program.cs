using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Wallet.Data.Db;
using Wallet.Data.Helpers.Contracts;
using Wallet.Data.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Data.Repositories.Implementations;
using Wallet.Services.Contracts;
using Wallet.Services.Encryption;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Factory;
using Wallet.Services.HostedServices;
using Wallet.Services.Implementations;
using Wallet.Services.Models;
using Wallet.Services.Validation.CardValidation;
using Wallet.Services.Validation.TransactionValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Wallet.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure DbContext
            builder.Services.AddDbContext<ApplicationContext>(options =>
            {
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
            });

            // Add Identity services
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                // Configure lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lockout duration
                options.Lockout.MaxFailedAccessAttempts = 5; // Number of failed attempts allowed before lockout
                options.Lockout.AllowedForNewUsers = true; // Lockout new users by default
            })
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders()
            .AddRoles<IdentityRole>();

            // Add Authentication and JWT Bearer token services (not typically needed in MVC)
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                     ValidAudience = builder.Configuration["JwtSettings:Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
                 };

                 options.Events = new JwtBearerEvents
                 {
                     OnMessageReceived = context =>
                     {
                         // Extract token from the HttpOnly cookie
                         context.Token = context.Request.Cookies["AuthToken"];
                         return Task.CompletedTask;
                     },
                     OnAuthenticationFailed = context =>
                     {
                         Console.WriteLine("Authentication failed: " + context.Exception.Message);
                         return Task.CompletedTask;
                     }
                 };
             })
             .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
             {
                 options.LoginPath = "/Account/Login";
                 options.LogoutPath = "/Account/Logout";
                 options.ExpireTimeSpan = TimeSpan.FromDays(1);
                 options.SlidingExpiration = true;
                 options.Cookie.HttpOnly = true;
                 options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use Always in production
             });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            });

            // Add Controllers and Views
            builder.Services.AddControllersWithViews();

            // Register services and dependencies
            ConfigureServices(builder.Services, builder.Configuration);
            builder.Services.AddScoped<IOverdraftSettingsRepository, OverdraftSettingsRepository>();
            builder.Services.AddScoped<IOverdraftSettingsService, OverdraftSettingsService>();



            // Seed roles and users


            var apiKey = builder.Configuration["CurrencyLayer:ApiKey"];

            // Ensure the HttpClient and other necessary dependencies are registered correctly
            builder.Services.AddHttpClient<ICurrencyExchangeService, CurrencyExchangeService>(client =>
            {
                client.BaseAddress = new Uri("https://api.currencylayer.com/");
            });

            // If the constructor of CurrencyExchangeService requires more parameters (like apiKey), inject it directly
            builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                return new CurrencyExchangeService(httpClient, apiKey);
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                SeedRolesOnce(roleManager, userManager).Wait(); // Blocking wait since Main is not async
            }

            // Call the seed method
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    ApplicationDbContextSeed.SeedAsync(services).Wait();
                }
                catch (Exception ex)
                {
                    // Handle exceptions during seeding
                    Console.WriteLine($"An error occurred seeding the DB: {ex.Message}");
                }
            }

            // Enable HTTPS Redirection
            app.UseHttpsRedirection();

            // Enable Routing, Authentication, and Authorization
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register your custom services, repositories, and other dependencies

            // Register Cloudinary configuration
            var cloudinaryConfig = configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
            services.AddSingleton(cloudinaryConfig);

            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<CloudinarySettings>();
                return new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
            });

            // Register CloudinaryService
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Register other services
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<TwilioVerifyService>();

            // Repositories
            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IContactsRepository, ContactRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOverdraftSettingsRepository, OverdraftSettingsRepository>();

            // Services
            services.AddScoped<ICardService, CardService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOverdraftSettingsService, OverdraftSettingsService>();

            // Factories
            services.AddScoped<ICardFactory, CardFactory>();
            services.AddScoped<IWalletFactory, WalletFactory>();
            services.AddScoped<ITransactionFactory, TransactionFactory>();
            services.AddScoped<IContactFactory, ContactFactory>();
            services.AddScoped<ICategoryFactory, CategoryFactory>();

            // Validators
            services.AddScoped<CardValidation>();
            services.AddScoped<ITransactionValidator, TransactionValidator>();

            // Hosted services
            services.AddHostedService<RecurringTransactionHostedService>();
            services.AddHostedService<UserBlockUnblockService>();

            // Caching
            services.AddMemoryCache();
            services.AddScoped<VerifyEmailService>();
            services.AddScoped<IAuthManager, AuthManager>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();

        }

        private static async Task SeedRolesOnce(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            // Define roles to be created
            string[] roleNames = { "Admin", "User", "Blocked" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
