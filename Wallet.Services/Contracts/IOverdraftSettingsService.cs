namespace Wallet.Services.Contracts
{
    public interface IOverdraftSettingsService
    {
        Task<bool> SetConsecutiveNegativeMonthsAsync(int months);
        Task<bool> SetInterestRateAsync(decimal newRate);
        Task<bool> SetOverdraftLimitAsync(decimal newLimit);
    }
}