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
using Wallet.Services.Validation.CardValidation;

namespace Wallet.Services.Implementations
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICardFactory _cardFactory;
        private readonly CardValidation _cardValidation;
        //private readonly IEncryptionService _encryptionService;

        public CardService(ICardRepository cardRepository, ICardFactory cardFactory, CardValidation cardValidation)
        {
            _cardRepository = cardRepository;
            _cardFactory = cardFactory;
            _cardValidation = cardValidation;
            //_encryptionService = encryptionService;
        }

        

        public async Task AddCardAsync(CardRequest cardRequest, string userID)
        {
            var validationResult = _cardValidation.Validate(cardRequest);

            if (!validationResult.IsValid)
            {
                // Handle validation errors
                throw new ArgumentException(string.Join("; ", validationResult.Errors));
            }

            var card = _cardFactory.Map(cardRequest, userID);
            card.CardNetwork = validationResult.CardNetwork;
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
