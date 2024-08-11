using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Data.Models.Transactions;

namespace Wallet.Data.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserByIdAsync(string userId);
        Task<List<AppUser>> SearchUsersAsync(string? searchTerm, int page, int pageSize);
        Task<int> GetUserCountAsync(string searchTerm);
        Task<bool> UpdateProfilePictureAsync(string userId, string pictureURL);
    }

}
