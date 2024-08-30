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

        Task<ICollection<ContactResponseDTO>> SearchForContactsAsync(string userId, string searchQuery);
    }
}
