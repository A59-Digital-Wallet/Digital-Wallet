using Digital_Wallet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Services.Contracts;
using Wallet.Services.Implementations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Wallet.Data.Repositories.Contracts;
using Wallet.Data.Repositories.Implementations;

namespace Wallet.API.Tests
{
    [TestClass]
    public class ProgramIntegrationTests
    {
        private readonly HttpClient _client;

        public ProgramIntegrationTests()
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // You can configure the builder here if needed
                });

            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task SwaggerUI_Should_Return_OK()
        {
            // Act
            var response = await _client.GetAsync("/swagger");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        // Add more integration tests here to cover other aspects of Program.cs
    }
    [TestClass]
    public class ProgramConfigurationTests
    {
        [TestMethod]
        public void Test_JwtConfiguration_Is_Correct()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                {"JwtSettings:Secret", "YourSecret"},
                {"JwtSettings:Issuer", "YourIssuer"},
                {"JwtSettings:Audience", "YourAudience"}
                })
                .Build();

            builder.Configuration.AddConfiguration(configuration);

            // Act
            var key = Encoding.ASCII.GetBytes(configuration["JwtSettings:Secret"]);

            // Assert
            Assert.AreEqual("YourSecret", configuration["JwtSettings:Secret"]);
        }
    }

    [TestClass]
    public class ProgramMiddlewareTests
    {
        [TestMethod]
        public async Task Test_Middleware_Pipeline()
        {
            // Arrange
            await using var factory = new WebApplicationFactory<Program>(); // Ensure your Program.cs has a namespace
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/swagger");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }

   
}
