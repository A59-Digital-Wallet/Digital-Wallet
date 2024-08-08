using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Data.Repositories.Implementations;
using Wallet.Services.Contracts;
using Wallet.Services.Encryption;
using Wallet.Services.Factory;
using Wallet.Services.Factory.Contracts;
using Wallet.Services.Implementations;
using static System.Net.WebRequestMethods;

namespace Digital_Wallet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationContext>(options =>
            {
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
            });

            // Add controllers
            builder.Services.AddControllers();
            builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
            builder.Services.AddAuthorizationBuilder();
            builder.Services.AddIdentityCore<AppUser>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddApiEndpoints();

            // Configure Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            // Repositories
            builder.Services.AddScoped<ICardRepository, CardRepository>();

            // Services
            builder.Services.AddScoped<ICardService, CardService>();
            //builder.Services.AddScoped<IEncryptionService, EncryptionService>();

            // Factories
            builder.Services.AddScoped<ICardFactory, CardFactory>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapIdentityApi<AppUser>();
            // Enable HTTPS Redirection
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
