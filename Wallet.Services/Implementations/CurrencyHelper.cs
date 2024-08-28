namespace Wallet.Services.Implementations
{
    public static class CurrencyHelper
    {
        public static string GetCurrencyCulture(string currencyCode)
        {
            return currencyCode switch
            {
                "USD" => "en-US",
                "EUR" => "fr-FR",
                "BGN" => "bg-BG",
                "GBP" => "en-GB",
                // Add more currencies as needed
                _ => "en-US", // Default to USD if currency is unknown
            };
        }
    }
}
