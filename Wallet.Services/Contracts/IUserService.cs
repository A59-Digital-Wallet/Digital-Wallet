using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Factory;

namespace Wallet.Services.Contracts
{
    public interface IUserService
    {
        Task<AppUser> GetUserByIdAsync(string userId);
        Task<PagedResult<AppUser>> SearchUsersAsync(string searchTerm, int page, int pageSize);
        Task UploadProfilePictureAsync(string userId, IFormFile file);


    }
}
