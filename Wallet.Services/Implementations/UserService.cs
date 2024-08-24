using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Wallet.Data.Models;
using Wallet.Data.Models.Enums;
using Wallet.Data.Repositories.Contracts;
using Wallet.Data.Repositories.Implementations;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory;
using IEmailSender = Wallet.Services.Contracts.IEmailSender;

namespace Wallet.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly TwilioVerifyService _verifyService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public UserService(IUserRepository userRepository, UserManager<AppUser> userManager, ICloudinaryService cloudinaryService, SignInManager<AppUser> signInManager, IEmailSender emailSender, TwilioVerifyService verifyService, ITwoFactorAuthService twoFactorAuthService)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _verifyService = verifyService;
            _twoFactorAuthService = twoFactorAuthService;
        }
        public async Task<IdentityResult> RegisterUserAsync(RegisterModel model)
        {
            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                // Generate a 6-digit code
                var confirmationCode = new Random().Next(100000, 999999).ToString();
                user.EmailConfirmationCode = confirmationCode;
                user.EmailConfirmationCodeGeneratedAt = DateTime.UtcNow;

                // Update user with confirmation code
                await _userManager.UpdateAsync(user);

                // Send the code via email
                await _emailSender.SendEmail(
                    "Email Confirmation",
                    user.Email,
                    user.FirstName,
                    $"Hello, please confirm your email by entering the following code: {confirmationCode}"
                );

                // Uncomment the code below if phone verification is required
                /*
                var phoneVerificationSent = await SendPhoneVerificationCodeAsync(user.PhoneNumber);
                if (!phoneVerificationSent)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "Failed to send phone verification code."
                    });
                }
                */

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return IdentityResult.Success;
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return IdentityResult.Success;
                }
            }

            return result;
        }
        public async Task<bool> VerifyEmailAsync(VerifyEmailModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return false;
            }

            // Check if the code matches and is within a valid timeframe (e.g., 10 minutes)
            if (user.EmailConfirmationCode == model.Code &&
                user.EmailConfirmationCodeGeneratedAt.HasValue &&
                (DateTime.UtcNow - user.EmailConfirmationCodeGeneratedAt.Value).TotalMinutes <= 10)
            {
                user.EmailConfirmed = true;
                user.EmailConfirmationCode = null;
                user.EmailConfirmationCodeGeneratedAt = null;
                await _userManager.UpdateAsync(user);

                return true;
            }

            return false;
        }
        public async Task<bool> VerifyPhoneAsync(string phoneNumber, string code)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                return false;
            }

            var verified = await _verifyService.VerifyCodeAsync(phoneNumber, code);
            if (verified)
            {
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
                return true;
            }

            return false;
        }


        public async Task<AppUser> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<PagedResult<UserWithRolesDto>> SearchUsersAsync(string? searchTerm, int page, int pageSize)
        {
            // Fetch the users using your existing logic
            var users = await _userRepository.SearchUsersAsync(searchTerm, page, pageSize);
            var totalUsers = await _userRepository.GetUserCountAsync(searchTerm);

            // Prepare a list of users with their roles
            var usersWithRoles = new List<UserWithRolesDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesDto
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return new PagedResult<UserWithRolesDto>
            {
                Items = usersWithRoles,
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

        public async Task<IdentityResult> ManageRoleAsync(string userId, string action)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRole = userRoles.FirstOrDefault()?.ToLower(); // Get the current role of the user, if any

            IdentityResult result = IdentityResult.Success;

            switch (action.ToLower())
            {
                case "block":
                    if (currentRole == "admin")
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Description = "Admin users cannot be blocked."
                        });
                    }
                    if (currentRole != null && currentRole != "blocked")
                    {
                        result = await UnassignRoleAsync(user, currentRole);
                        if (!result.Succeeded) return result;
                    }
                    result = await AssignRoleAsync(user, "blocked");
                    break;

                case "unblock":
                    if (currentRole == "blocked")
                    {
                        result = await UnassignRoleAsync(user, "blocked");
                    }
                    result = await AssignRoleAsync(user, "user");
                    break;

                case "makeadmin":
                    if (currentRole != "admin")
                    {
                        if (currentRole != null)
                        {
                            result = await UnassignRoleAsync(user, currentRole);
                            if (!result.Succeeded) return result;
                        }
                        result = await AssignRoleAsync(user, "admin");
                    }
                    break;

                case "user":
                    if (currentRole != "user")
                    {
                        if (currentRole != null)
                        {
                            result = await UnassignRoleAsync(user, currentRole);
                            if (!result.Succeeded) return result;
                        }
                        result = await AssignRoleAsync(user, "user");
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid action. Use 'block', 'unblock', 'user' or 'makeadmin'.");
            }

            return result;
        }

        private async Task<IdentityResult> AssignRoleAsync(AppUser user, string role)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User is already in the '{role}' role."
                });
            }

            return await _userManager.AddToRoleAsync(user, role);
        }

        private async Task<IdentityResult> UnassignRoleAsync(AppUser user, string role)
        {
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User is not in the '{role}' role."
                });
            }

            return await _userManager.RemoveFromRoleAsync(user, role);
        }
        private async Task<bool> SendPhoneVerificationCodeAsync(string phoneNumber)
        {
            // Logic to send a phone verification code
            return await _verifyService.SendVerificationCodeAsync(phoneNumber);
        }
        public async Task<IdentityResult> UpdateUserProfileAsync(UpdateUserModel model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "User not found."
                });
            }

            if (user.PhoneNumber != model.PhoneNumber)
            {
                user.PhoneNumber = model.PhoneNumber;
                user.PhoneNumberConfirmed = false;

                // Uncomment the code below if phone verification is required
                /*
                var phoneVerificationSent = await SendPhoneVerificationCodeAsync(model.PhoneNumber);
                if (!phoneVerificationSent)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "Failed to send phone verification code."
                    });
                }
                */
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            return await _userManager.UpdateAsync(user);
        }
        public async Task<(AppUser user, bool requiresTwoFactor)> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    return (user, true);
                }

                return (user, false);
            }

            return (null, false);
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(AppUser user, string code)
        {
            return await _twoFactorAuthService.VerifyTwoFactorCodeAsync(user, code);
        }
    }
}
