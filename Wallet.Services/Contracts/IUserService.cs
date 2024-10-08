﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Wallet.Data.Models;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Factory;

namespace Wallet.Services.Contracts
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterModel model);
        Task<bool> VerifyEmailAsync(VerifyEmailModel model);
        Task<bool> VerifyPhoneAsync(string phoneNumber, string code);
        Task<AppUser> GetUserByIdAsync(string userId);
        Task<PagedResult<UserWithRolesDto>> SearchUsersAsync(string searchTerm, int page, int pageSize);
        Task UploadProfilePictureAsync(string userId, IFormFile file);
        Task<IdentityResult> ManageRoleAsync(string userId, string action);
        Task<IdentityResult> UpdateUserProfileAsync(UpdateUserModel model, string userId);
        Task<(AppUser user, bool requiresTwoFactor)> LoginAsync(LoginModel model);
        Task<bool> VerifyTwoFactorCodeAsync(AppUser user, string code);
    }
}
