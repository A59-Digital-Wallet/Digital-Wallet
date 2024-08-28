using Wallet.Data.Models.Enums;

namespace Wallet.Services.Contracts
{
    public interface ICurrencyExchangeService
    {
        Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency);
    }

}
