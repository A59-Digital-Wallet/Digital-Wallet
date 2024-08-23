using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.DTO.Response;

namespace Wallet.Services.Contracts
{
    public interface IContactService
    {
        Task<ICollection<ContactResponseDTO>> GetContactsAsync(string userId);
        Task AddContactAsync(string userId, string contactId);
        Task<bool> RemoveContactAsync(string userId, string contactId);
        Task<Contact> GetContactAsync(string userId, string contactId);
    }
}
