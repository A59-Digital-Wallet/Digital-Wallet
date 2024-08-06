using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;
using Wallet.Data.Db;
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

            // Configure Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Enable HTTPS Redirection
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
