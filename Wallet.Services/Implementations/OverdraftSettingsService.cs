using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<bool> SetInterestRateAsync(decimal newRate)
        {
            OverdraftSettings settings = await _repository.GetSettingsAsync();

            newRate = newRate / 100;

            if (settings != null)
            {
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
