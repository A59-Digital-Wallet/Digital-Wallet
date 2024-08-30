﻿using Wallet.Common.Exceptions;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Implementations
{
    public class ContactService : IContactService
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly IContactFactory _contactsFactory;
        private readonly IUserRepository _userRepository;

        public ContactService(IContactsRepository contactsRepository, IContactFactory contactsFactory, IUserRepository userRepository)
        {
            _contactsRepository = contactsRepository;
            _contactsFactory = contactsFactory;
            _userRepository = userRepository;
        }

        public async Task<ICollection<ContactResponseDTO>> GetContactsAsync(string userId)
        {
            ICollection<Contact> contacts = await _contactsRepository.GetContactsAsync(userId);

            //Should uncomment when I handle the exception in MVC 

            /*            if (contacts.IsNullOrEmpty())
                        {
                            throw new EntityNotFoundException(Messages.Service.ContactsNotFound);
                        }*/

            ICollection<ContactResponseDTO> contactsResponse = _contactsFactory.Map(contacts);
            return contactsResponse;
        }

        public async Task AddContactAsync(string userId, string contactId)
        {
            AppUser contactUser = await _userRepository.GetUserByIdAsync(contactId);

            if (contactUser == null)
            {
                throw new EntityNotFoundException(Messages.Service.ContactNotFound);
            }

            Contact existingContact = await _contactsRepository.GetContactAsync(userId, contactId);
            if (existingContact != null)
            {
                throw new InvalidOperationException(Messages.Service.ContactAlreadyExists);
            }
            Contact contact = _contactsFactory.Map(userId, contactId);
            await _contactsRepository.AddContactAsync(contact);
        }
        public async Task<ICollection<ContactResponseDTO>> SearchForContactsAsync(string userId, string searchQuery)
        {
            ICollection<Contact> contacts = await _contactsRepository.SearchForContactsAsync(userId, searchQuery);

            //Should uncomment when I handle the exception in MVC 

            /*            if (contacts.IsNullOrEmpty())
                        {
                            throw new EntityNotFoundException(Messages.Service.ContactsNotFound);
                        }*/

            ICollection<ContactResponseDTO> contactsResponse = _contactsFactory.Map(contacts);
            return contactsResponse;
        }


        public async Task<bool> RemoveContactAsync(string userId, string contactId)
        {
            Contact contact = await _contactsRepository.GetContactAsync(userId, contactId);
            if (contact == null)
            {
                throw new EntityNotFoundException(Messages.Service.ContactAlreadyExists);
            }
            await _contactsRepository.RemoveContactAsync(contact);
            return true;
        }
        public async Task<Contact> GetContactAsync(string userId, string contactId)
        {
            return await _contactsRepository.GetContactAsync(userId, contactId);
        }

    }
}
