using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Card>> GetCardsAsync(string userId)
        {

            var cards = await _context.Cards
                .Where(c => c.AppUserId == userId) 
                .ToListAsync();
            return cards;
        }

        public async Task<Card> GetCardAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            return card;
        }

        public async Task AddCardAsync(Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CardExistsAsync(string userId, string encryptedCardNumber)
        {
            return await _context.Cards.AnyAsync(c => c.AppUserId == userId && c.CardNumber == encryptedCardNumber);
        }

        public async Task DeleteCardAsync(Card card)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }

    }
}
