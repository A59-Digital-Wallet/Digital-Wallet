using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Services.Contracts;

namespace Wallet.Services.Tests
{
    public class MockCurrencyExchangeService
    {
        public Mock<ICurrencyExchangeService> GetMockService()
        {
            var mockService = new Mock<ICurrencyExchangeService>();

            // Setup ConvertAsync to return a specific exchange rate
            mockService.Setup(service => service.ConvertAsync(It.IsAny<decimal>(), It.IsAny<Currency>(), It.IsAny<Currency>()))
                       .ReturnsAsync((decimal amount, Currency fromCurrency, Currency toCurrency) =>
                       {
                           // Example: Different rates for different currencies
                           if (fromCurrency == Currency.USD && toCurrency == Currency.EUR)
                           {
                               return amount * 0.85m; // Example rate: 1 USD = 0.85 EUR
                           }
                           else if (fromCurrency == Currency.EUR && toCurrency == Currency.USD)
                           {
                               return amount * 1.2m; // Example rate: 1 EUR = 1.2 USD
                           }
                           else
                           {
                               return amount; // Default case if currencies are the same or unknown
                           }
                       });

            return mockService;
        }
    }
}
