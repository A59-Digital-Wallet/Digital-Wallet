using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface IStatsService
    {
        Task<StatsViewModel> GetUserStatsAsync(string userId, DateTime? startDate, DateTime? endDate);
    }
}
