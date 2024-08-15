using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;

        public AccountService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }


       

       
        public async Task<string> GetOrGenerateAuthenticatorKeyAsync(AppUser user)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            return key;
        }


        public async Task<bool> IsTwoFactorEnabledAsync(AppUser user)
        {
            return await _userManager.GetTwoFactorEnabledAsync(user);
        }

        public async Task<IEnumerable<string>> GetValidTwoFactorProvidersAsync(AppUser user)
        {
            return await _userManager.GetValidTwoFactorProvidersAsync(user);
        }

        public async Task<string> GetAuthenticatorKeyAsync(AppUser user)
        {
            return await _userManager.GetAuthenticatorKeyAsync(user);
        }

        public async Task ResetAuthenticatorKeyAsync(AppUser user)
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
        }

        public async Task<bool> VerifyTwoFactorTokenAsync(AppUser user, string code)
        {
            return await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
        }

        public async Task SetTwoFactorEnabledAsync(AppUser user, bool enabled)
        {
            await _userManager.SetTwoFactorEnabledAsync(user, enabled);
        }
    }
}
