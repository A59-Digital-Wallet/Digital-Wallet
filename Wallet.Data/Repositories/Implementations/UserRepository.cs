using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Db;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;
using Wallet.Data.Repositories.Contracts;

namespace Wallet.Data.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
   

        public UserRepository(UserManager<AppUser> userManager, ApplicationContext context)
        {
            _userManager = userManager;
            
        }

        public async Task<AppUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<AppUser>> SearchUsersAsync(string? searchTerm, int page, int pageSize)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.PhoneNumber.Contains(searchTerm));
            }

            return await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUserCountAsync(string? searchTerm)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.PhoneNumber.Contains(searchTerm));
            }

            return await usersQuery.CountAsync();
        }

      

      

        
    }

}
