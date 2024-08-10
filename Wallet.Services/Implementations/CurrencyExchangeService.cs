using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Wallet.Data.Models.Enums;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CurrencyExchangeService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (fromCurrency == toCurrency)
            {
                return amount;
            }

            decimal exchangeRate = await GetExchangeRateAsync(fromCurrency, toCurrency);
            return amount * exchangeRate;
        }

        private async Task<decimal> GetExchangeRateAsync(Currency fromCurrency, Currency toCurrency)
        {
            var url = $"https://api.currencylayer.com/convert?from={fromCurrency}&to={toCurrency}&amount=1&apikey={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var jsonResponse = JObject.Parse(response);
            if (jsonResponse["success"].Value<bool>())
            {
                return jsonResponse["info"]["rate"].Value<decimal>();
            }
            else
            {
                throw new Exception("Failed to retrieve exchange rate.");
            }
        }
    }
}
