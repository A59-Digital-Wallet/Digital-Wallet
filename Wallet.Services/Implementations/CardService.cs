using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Common.Exceptions;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
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
        private readonly IEncryptionService _encryptionService;

        public CardService(ICardRepository cardRepository, ICardFactory cardFactory, CardValidation cardValidation, IEncryptionService encryptionService)
        {
            _cardRepository = cardRepository;
            _cardFactory = cardFactory;
            _cardValidation = cardValidation;
            _encryptionService = encryptionService;
        }

        public async Task<List<CardResponseDTO>> GetCardsAsync(string userId)
        {
            List<Card> cards = await _cardRepository.GetCardsAsync(userId);

            if(cards == null)
            {
                throw new EntityNotFoundException("No cards were found");
            }
            if (cards.Count > 0)
            {
                foreach (var card in cards)
                {
                    card.CardNumber = await _encryptionService.DecryptAsync(card.CardNumber);
                    card.CVV = await _encryptionService.DecryptAsync(card.CVV);
                }
            }
           

            List<CardResponseDTO> cardResponseDTOs = _cardFactory.Map(cards);
            return cardResponseDTOs;
        }

        public async Task AddCardAsync(CardRequest cardRequest, string userID)
        {
            var validationResult = _cardValidation.Validate(cardRequest);

            if (!validationResult.IsValid)
            {
                throw new ArgumentException(string.Join("; ", validationResult.Errors));
            }

            string encryptedCardNumber = await _encryptionService.EncryptAsync(cardRequest.CardNumber);

            bool isDuplicate = await _cardRepository.CardExistsAsync(userID, encryptedCardNumber);
            if (isDuplicate)
            {
                throw new InvalidOperationException("This card has already been added.");
            }

            var card = _cardFactory.Map(cardRequest, userID, validationResult.CardNetwork);
            card.CardNumber = encryptedCardNumber;
            card.CVV = await _encryptionService.EncryptAsync(card.CVV);
            await _cardRepository.AddCardAsync(card);
        }

        public async Task<CardResponseDTO> GetCardAsync(int cardId, string userID)
        {
            var card = await _cardRepository.GetCardAsync(cardId);

            if (userID != card.AppUserId)
            {
                throw new AuthorizationException("You cannot access this card!");
            }

            if (card != null)
            {
                card.CardNumber = await _encryptionService.DecryptAsync(card.CardNumber);
                card.CVV = await _encryptionService.DecryptAsync(card.CVV);
            }
            var cardDTO = _cardFactory.Map(card);
            return cardDTO;
        }

        public async Task<bool> DeleteCardAsync(int cardId, string userId)
        {
            var card = await _cardRepository.GetCardAsync(cardId);

            if(card == null)
            {
                throw new ArgumentException("Card not found.");
            }
            if (card.AppUserId != userId)
            {
                throw new ArgumentException("Ýou are not authorized to delete this card.");
            }


           await _cardRepository.DeleteCardAsync(card);
           return true;
        }
    }
}
