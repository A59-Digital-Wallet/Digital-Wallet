using Wallet.Data.Models;
using Wallet.DTO.Response;

namespace Wallet.Services.Factory.Contracts
{
    public interface IContactsFactory
    {
        ICollection<ContactResponseDTO> Map(ICollection<Contact> contacts);
        Contact Map(string userId, string contactId);
    }
}