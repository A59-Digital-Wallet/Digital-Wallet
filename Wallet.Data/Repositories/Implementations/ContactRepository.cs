using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Db;
using Wallet.Data.Migrations;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Data.Repositories.Implementations
{
    public class ContactRepository : IContactsRepository
    {
        private readonly ApplicationContext _context;
        public ContactRepository(ApplicationContext context)
        {
            _context = context;
        }


        public async Task<ICollection<Contact>> GetContactsAsync(string userId)
        {
            return await _context.Contacts
                .Where(u => u.UserId == userId)
                .Include(u => u.ContactUser)
                .ToListAsync();
        }

        public async Task<Contact> GetContactAsync(string userId, string contactId)
        {
            return await _context.Contacts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ContactUserId == contactId);
        }

        public async Task AddContactAsync(Contact contact)
        {
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveContactAsync(Contact contact)
        {
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
        }

    }
}
