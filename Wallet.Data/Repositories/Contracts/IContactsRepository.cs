using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.Data.Repositories.Contracts
{
    public interface IContactsRepository
    {
        Task<ICollection<Contact>> GetContactsAsync(string userId);
        Task<Contact> GetContactAsync(string userId, string contactId);
        Task AddContactAsync(Contact contact);
        Task RemoveContactAsync(Contact contact);
    }
}
