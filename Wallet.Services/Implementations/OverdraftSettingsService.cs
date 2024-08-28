using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class OverdraftSettingsService : IOverdraftSettingsService
    {
        private readonly IOverdraftSettingsRepository _repository;

        public OverdraftSettingsService(IOverdraftSettingsRepository repository)
        {
            _repository = repository;
        }

        public async Task<OverdraftSettings> GetSettingsAsync()
        {
            return await _repository.GetSettingsAsync();
        }

        public async Task<bool> SetInterestRateAsync(decimal newRate)
        {
            OverdraftSettings settings = await _repository.GetSettingsAsync();


            if (settings != null)
            {
                newRate = newRate / 100;
                settings.DefaultInterestRate = newRate;
                return await _repository.UpdateSettings(settings);
            }
            return false;
        }

        public async Task<bool> SetOverdraftLimitAsync(decimal newLimit)
        {
            OverdraftSettings settings = await _repository.GetSettingsAsync();

            if (settings != null)
            {
                settings.DefaultOverdraftLimit = newLimit;
                return await _repository.UpdateSettings(settings);
            }
            return false;
        }
        public async Task<bool> SetConsecutiveNegativeMonthsAsync(int months)
        {
            OverdraftSettings settings = await _repository.GetSettingsAsync();

            if (settings != null)
            {
                settings.DefaultConsecutiveNegativeMonths = months;
                return await _repository.UpdateSettings(settings);
            }
            return false;
        }
    }
}
