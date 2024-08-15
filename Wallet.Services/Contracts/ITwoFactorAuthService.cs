using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Data.Models;

namespace Wallet.Services.Contracts
{
    public interface ITwoFactorAuthService
    {
        Task<string> GenerateQrCodeUriAsync(AppUser user);
        Task<bool> VerifyTwoFactorCodeAsync(AppUser user, string code);
        Task EnableTwoFactorAuthenticationAsync(AppUser user);
        Task DisableTwoFactorAuthenticationAsync(AppUser user);
        Task<byte[]> GenerateQrCodeImageAsync(AppUser user);
        
    }
}
