using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.Data.Repositories.Contracts
{
    public interface IMoneyRequestRepository
    {
        Task CreateAsync(MoneyRequest moneyRequest);
        Task<MoneyRequest> GetByIdAsync(int id);
        Task<IEnumerable<MoneyRequest>> GetReceivedRequestsAsync(string recipientId);
        Task UpdateAsync(MoneyRequest moneyRequest);
    }
}
