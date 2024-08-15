using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.Services.Contracts
{
    public interface IAccountService
    {
        
       
        Task<bool> IsTwoFactorEnabledAsync(AppUser user);
        Task<IEnumerable<string>> GetValidTwoFactorProvidersAsync(AppUser user);
        Task<string> GetAuthenticatorKeyAsync(AppUser user);
        Task ResetAuthenticatorKeyAsync(AppUser user);
        Task<bool> VerifyTwoFactorTokenAsync(AppUser user, string code);
        Task SetTwoFactorEnabledAsync(AppUser user, bool enabled);
        Task<string> GetOrGenerateAuthenticatorKeyAsync(AppUser user);
    }
}
