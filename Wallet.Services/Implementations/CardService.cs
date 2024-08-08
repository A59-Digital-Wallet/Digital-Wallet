using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Implementations
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICardFactory _cardFactory;
        //private readonly IEncryptionService _encryptionService;

        public CardService(ICardRepository cardRepository, ICardFactory cardFactory)
        {
            _cardRepository = cardRepository;
            _cardFactory = cardFactory;
            //_encryptionService = encryptionService;
        }

        public async Task AddCardAsync(CardRequest cardRequest)
        {
            var card = _cardFactory.Map(cardRequest);
            //Uncomment when encryption is fully set up.
            //card.CardNumber = await _encryptionService.EncryptAsync(card.CardNumber);
            //card.CVV = await _encryptionService.EncryptAsync(card.CVV);
            await _cardRepository.AddCardAsync(card);
        }

        public async Task<Card> GetCardAsync(int cardId)
        {
            var card = await _cardRepository.GetCardAsync(cardId);
            
            //Uncomment when encryption is fully set up.
            //if (card != null)
            //{
            //    card.CardNumber = await _encryptionService.DecryptAsync(card.CardNumber);
            //    card.CVV = await _encryptionService.DecryptAsync(card.CVV);
            //}
            return card;
        }
    }
}
