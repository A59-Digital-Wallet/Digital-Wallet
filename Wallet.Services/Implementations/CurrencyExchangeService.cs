using Newtonsoft.Json.Linq;
using Wallet.Common.Helpers;
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
            try
            {
                // Use the source parameter to directly get the exchange rate from fromCurrency to toCurrency
                var url = $"https://api.currencylayer.com/live?access_key={_apiKey}&currencies={toCurrency}&source={fromCurrency}&format=1";
                var response = await _httpClient.GetStringAsync(url);
                var jsonResponse = JObject.Parse(response);

                if (jsonResponse["success"].Value<bool>())
                {
                    string rateKey = $"{fromCurrency}{toCurrency}";
                    decimal exchangeRate = jsonResponse["quotes"][rateKey].Value<decimal>();

                    return exchangeRate;
                }
                else
                {
                    throw new Exception($"Currency conversion failed: {jsonResponse["error"]["info"].ToString()}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(Messages.Service.FailedDueToNetworkError, ex);
            }
            catch (Exception ex)
            {
                throw new Exception(Messages.Service.FailedToRetrieveRate, ex);
            }
        }
    }
}
