using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface IStatsService
    {
        Task<StatsViewModel> GetUserStatsAsync(string userId, DateTime? startDate, DateTime? endDate);
        Task<(List<string>, List<decimal>)> GetBalanceOverTime(int walletId, string interval, string userId);
    }
}
