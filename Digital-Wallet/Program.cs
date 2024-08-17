using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Org.BouncyCastle.Asn1.Pkcs;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Twilio.Clients;
using Wallet.Data.Db;
using Wallet.Data.Helpers;
using Wallet.Data.Helpers.Contracts;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Data.Repositories.Implementations;
using Wallet.Services.Contracts;
using Wallet.Services.Encryption;
using Wallet.Services.Factory;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.HostedServices;
using Wallet.Services.Implementations;
using Wallet.Services.Models;
using Wallet.Services.Validation.CardValidation;
using Wallet.Services.Validation.TransactionValidation;

namespace Digital_Wallet
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

            // Add Authentication and JWT Bearer token services
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

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
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            });


            // Add Controllers
            builder.Services.AddControllers();

            var apiKey = builder.Configuration["CurrencyLayer:ApiKey"];

            // Add services to the container.
            builder.Services.AddHttpClient<ICurrencyExchangeService, CurrencyExchangeService>(client =>
            {
                client.BaseAddress = new Uri("https://api.currencylayer.com/");
            });

            builder.Services.AddSingleton<ICurrencyExchangeService, CurrencyExchangeService>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new CurrencyExchangeService(httpClient, apiKey);
            });

            var cloudinaryConfig = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
            builder.Services.AddSingleton(cloudinaryConfig);

            builder.Services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<CloudinarySettings>();
                return new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
            });

            // Register CloudinaryService
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Add Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter 'Bearer' followed by your token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<TwilioVerifyService>();

            // Repositories
            builder.Services.AddScoped<ICardRepository, CardRepository>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IContactsRepository, ContactRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IOverdraftSettingsRepository, OverdraftSettingsRepository>();

            // Services
            builder.Services.AddScoped<ICardService, CardService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<IUserService, UserService>(); 
            builder.Services.AddScoped<IContactService, ContactService>();
            builder.Services.AddScoped<IEncryptionService, EncryptionService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOverdraftSettingsService, OverdraftSettingsService>();

            // Factories
            builder.Services.AddScoped<ICardFactory, CardFactory>();
            builder.Services.AddScoped<IWalletFactory, WalletFactory>();
            builder.Services.AddScoped<ITransactionFactory, TransactionFactory>();
            builder.Services.AddScoped<IContactFactory, ContactFactory>();
            builder.Services.AddScoped<ICategoryFactory, CategoryFactory>();

            
            builder.Services.AddScoped<CardValidation>();
            builder.Services.AddScoped<ITransactionValidator, TransactionValidator>();
            builder.Services.AddScoped<SavingsInterestService>();
            builder.Services.AddHostedService<InterestHostedService>();
            builder.Services.AddHostedService<RecurringTransactionHostedService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<VerifyEmailService>();
            builder.Services.AddScoped<IAuthManager, AuthManager>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();

            var app = builder.Build();
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

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable HTTPS Redirection
            app.UseHttpsRedirection();

            // Enable Routing, Authentication, and Authorization
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();

            app.Run();
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
