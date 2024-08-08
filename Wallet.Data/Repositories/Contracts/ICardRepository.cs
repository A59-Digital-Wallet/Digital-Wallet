using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.Data.Repositories.Contracts
{
    public interface ICardRepository
    {
        Task AddCardAsync(Card card);
        Task<Card> GetCardAsync(int cardId);

    }
}
