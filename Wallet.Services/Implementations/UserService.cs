using Microsoft.AspNetCore.Http;
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
        private readonly ICloudinaryService _cloudinaryService;

        public UserService(IUserRepository userRepository, UserManager<AppUser> userManager, ICloudinaryService cloudinaryService)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
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

        public async Task UploadProfilePictureAsync(string userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Invalid file format. Only .jpg, .jpeg, and .png are allowed.");
            }

            // Upload image to Cloudinary
            var uploadResult = await _cloudinaryService.UploadImageAsync(file);
            if (uploadResult == null)
                throw new Exception("Error uploading image.");

            // Update user's profile picture URL
            var result = await _userRepository.UpdateProfilePictureAsync(userId, uploadResult.Url);
            if (!result)
                throw new Exception("Failed to update profile picture.");
        }
    }
}
