using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface IStatsService
    {
        Task<StatsViewModel> GetUserStatsAsync(string userId, DateTime? startDate, DateTime? endDate);
    }
}
