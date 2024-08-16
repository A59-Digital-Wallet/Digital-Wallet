using Microsoft.EntityFrameworkCore;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Data.Repositories.Implementations
{
    public class OverdraftSettingsRepository : IOverdraftSettingsRepository
    {
        private readonly ApplicationContext _context;
        public OverdraftSettingsRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<OverdraftSettings> GetSettingsAsync()
        {
            return await _context.OverdraftSettings.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateSettings(OverdraftSettings settings)
        {
            _context.Update(settings);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
