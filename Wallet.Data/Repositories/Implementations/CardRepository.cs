using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Data.Repositories.Implementations
{
    public class CardRepository : ICardRepository
    {
        private readonly ApplicationContext _context;
        public CardRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task AddCardAsync(Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
        }

        public async Task<Card> GetCardAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            return card;
        }

    }
}
