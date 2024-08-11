using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.Services.Contracts;
using Wallet.Services.Factory;

namespace Wallet.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;

        public UserService(IUserRepository userRepository, UserManager<AppUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<AppUser> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<PagedResult<AppUser>> SearchUsersAsync(string? searchTerm, int page, int pageSize)
        {
            var users = await _userRepository.SearchUsersAsync(searchTerm, page, pageSize);
            var totalUsers = await _userRepository.GetUserCountAsync(searchTerm);

            return new PagedResult<AppUser>
            {
                Items = users,
                TotalCount = totalUsers,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        

       
    }
}
