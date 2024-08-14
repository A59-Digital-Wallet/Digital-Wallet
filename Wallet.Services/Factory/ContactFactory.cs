﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                UserId= userId,
                ContactUserId = contactId
            };
            return request;
        }
    }
}
