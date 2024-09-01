using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Db;
using Wallet.Data.Models.Enums;
using Wallet.Data.Models;
using Microsoft.EntityFrameworkCore;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Data.Repositories.Implementations
{
    public class MoneyRequestRepository : IMoneyRequestRepository
    {
        private readonly ApplicationContext _context;

        public MoneyRequestRepository(ApplicationContext context)
        {
             _context = context;
        }

        public async Task CreateAsync(MoneyRequest moneyRequest)
        {
            await _context.MoneyRequests.AddAsync(moneyRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<MoneyRequest> GetByIdAsync(int id)
        {
            return await _context.MoneyRequests.FindAsync(id);
        }

        public async Task<IEnumerable<MoneyRequest>> GetReceivedRequestsAsync(string recipientId)
        {
            return await _context.MoneyRequests
                .Where(mr => mr.RecipientId == recipientId && mr.Status == RequestStatus.Pending)
                .Include(r => r.Requester)
                .ToListAsync();
        }

        public async Task UpdateAsync(MoneyRequest moneyRequest)
        {
            _context.MoneyRequests.Update(moneyRequest);
            await _context.SaveChangesAsync();
        }
    }
}
