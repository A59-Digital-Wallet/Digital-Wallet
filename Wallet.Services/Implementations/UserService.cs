using Castle.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wallet.Common.Helpers;
using Wallet.Data.Models;
using Wallet.Data.Repositories.Contracts;
using Wallet.DTO.Request;
using Wallet.DTO.Response;
using Wallet.Services.Contracts;
using Wallet.Services.Factory;
using Wallet.Services.Models;
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
        private readonly string _defaultProfilePicture;

        public UserService(
            IUserRepository userRepository,
            UserManager<AppUser> userManager,
            ICloudinaryService cloudinaryService,
            SignInManager<AppUser> signInManager, 
            IEmailSender emailSender, 
            TwilioVerifyService verifyService, 
            ITwoFactorAuthService twoFactorAuthService, 
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _verifyService = verifyService;
            _twoFactorAuthService = twoFactorAuthService;
            _defaultProfilePicture = configuration["CloudinarySettings:DefaultProfilePictureUrl"];

        }

      
        public async Task<IdentityResult> RegisterUserAsync(RegisterModel model)
        {
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                throw new InvalidOperationException("Email is already in use.");
            }

            // Check if the phone number is already in use
            var existingUserByPhoneNumber =  _userManager.Users
                .FirstOrDefault(u => u.PhoneNumber == model.PhoneNumber);

            
            if (existingUserByPhoneNumber != null)
            {
                throw new InvalidOperationException("Phone number is already in use.");
            }
            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                ProfilePictureURL = _defaultProfilePicture
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
                throw new ArgumentException(Messages.Service.NoFileUploaded);

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(Messages.Service.InvalidFileFormat);
            }

            // Upload image to Cloudinary
            var uploadResult = await _cloudinaryService.UploadImageAsync(file);
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.Url))
            {
                throw new Exception(Messages.Service.ErrorUploadingFile);
            }

            // Update user's profile picture URL
            var result = await _userRepository.UpdateProfilePictureAsync(userId, uploadResult.Url);
            if (!result)
            {
                throw new Exception(Messages.Service.FailedToUpdateImage);
            }
        }

        public async Task<IdentityResult> ManageRoleAsync(string userId, string action)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException(Messages.UserNotFound);
            }
            IdentityResult result = null;
            switch (action.ToLower())
            {
                case "block":
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Description = Messages.Service.AdminCannotBeBlocked
                        });
                    }
                    result = await AssignRoleAsync(user, "Blocked");
                    break;
                case "unblock":
                    result = await UnassignRoleAsync(user, "Blocked");
                    break;
                case "makeadmin":
                    result = await AssignRoleAsync(user, "Admin");
                    break;
                default:
                    throw new ArgumentException(Messages.Service.InvalidUserAction);
            }
            return result;
        }

        private async Task<IdentityResult> AssignRoleAsync(AppUser user, string role)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = string.Format(Messages.Service.UserAlreadyInRole, role)
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
                    Description = string.Format(Messages.Service.UserIsNotInRole, role)
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
                    Description = Messages.UserNotFound
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
