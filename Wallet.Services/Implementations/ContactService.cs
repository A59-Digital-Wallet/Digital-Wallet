using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Contracts;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Response;
using Wallet.Common.Exceptions;
using IdentityServer4.Extensions;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Implementations;
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

/*            if(contacts.IsNullOrEmpty())
            {
                throw new EntityNotFoundException("Contacts list is empty");
            }*/

            ICollection<ContactResponseDTO> contactsResponse = _contactsFactory.Map(contacts);
            return contactsResponse;
        }

        public async Task AddContactAsync (string userId, string contactId)
        {
            AppUser contactUser = await _userRepository.GetUserByIdAsync(contactId);

            if(contactUser == null)
            {
                throw new EntityNotFoundException("Contact user not found");
            }

            Contact existingContact = await _contactsRepository.GetContactAsync(userId, contactId);
            if (existingContact != null)
            {
                throw new InvalidOperationException("This contact already exists.");
            }
            Contact contact = _contactsFactory.Map(userId, contactId);
            await _contactsRepository.AddContactAsync(contact);
        }

        public async Task<bool> RemoveContactAsync(string userId, string contactId)
        {
            Contact contact = await _contactsRepository.GetContactAsync(userId, contactId);
            if(contact == null)
            {
                throw new EntityNotFoundException("Contact user not found");
            }
            await _contactsRepository.RemoveContactAsync(contact);
            return true;
        }
    }
}
