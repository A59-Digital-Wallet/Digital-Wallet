using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Data.Models.Enums;
using Wallet.Services.Implementations;

namespace Wallet.Services.Tests.CurrencyExchangeServiceTests
{
    [TestClass]
    public class ConvertAsyncTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private CurrencyExchangeService _sut;

        [TestInitialize]
        public void Setup()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _sut = new CurrencyExchangeService(_httpClient, "fakeApiKey");
        }

        [TestMethod]
        public async Task ReturnSameAmount_When_FromAndToCurrencyAreTheSame()
        {
            // Arrange
            var amount = 100m;
            var fromCurrency = Currency.USD;
            var toCurrency = Currency.USD;

            // Act
            var result = await _sut.ConvertAsync(amount, fromCurrency, toCurrency);

            // Assert
            Assert.AreEqual(amount, result);
        }

        [TestMethod]
        public async Task ConvertAmount_When_FromAndToCurrencyAreDifferent()
        {
            // Arrange
            var amount = 100m;
            var fromCurrency = Currency.USD;
            var toCurrency = Currency.EUR;
            var expectedExchangeRate = 0.85m;
            var expectedConvertedAmount = amount * expectedExchangeRate;

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"success\":true, \"quotes\":{{\"{fromCurrency}{toCurrency}\":{expectedExchangeRate}}}}}")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _sut.ConvertAsync(amount, fromCurrency, toCurrency);

            // Assert
            Assert.AreEqual(expectedConvertedAmount, result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Currency conversion failed: Invalid API key.")]
        public async Task ThrowException_When_ApiReturnsError()
        {
            // Arrange
            var amount = 100m;
            var fromCurrency = Currency.USD;
            var toCurrency = Currency.EUR;

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"success\":false, \"error\":{\"info\":\"Invalid API key.\"}}")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(mockResponse);

            // Act
            await _sut.ConvertAsync(amount, fromCurrency, toCurrency);
        }
    }
}
