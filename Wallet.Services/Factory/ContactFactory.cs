using Wallet.Data.Models;
using Wallet.DTO.Response;
using Wallet.Services.Factory.Contracts;

namespace Wallet.Services.Factory
{
    public class ContactFactory : IContactFactory
    {
        public ICollection<ContactResponseDTO> Map(ICollection<Contact> contacts)
        {
            ICollection<ContactResponseDTO> contactResponseDTO = contacts.Select(c => new ContactResponseDTO
            {
                Id = c.ContactUser.Id,
                FirstName = c.ContactUser.FirstName,
                LastName = c.ContactUser.LastName,
                ProfilePictureURL = c.ContactUser.ProfilePictureURL
            }).ToList();

            return contactResponseDTO;
        }
        public Contact Map(string userId, string contactId)
        {
            Contact request = new Contact()
            {
                UserId = userId,
                ContactUserId = contactId
            };
            return request;
        }
    }
}
