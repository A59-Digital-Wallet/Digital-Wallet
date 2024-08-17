using Wallet.Data.Models;

namespace Wallet.Data.Repositories.Contracts
{
    public interface IOverdraftSettingsRepository
    {
        Task<OverdraftSettings> GetSettingsAsync();
        Task<bool> UpdateSettings(OverdraftSettings settings);
    }
}