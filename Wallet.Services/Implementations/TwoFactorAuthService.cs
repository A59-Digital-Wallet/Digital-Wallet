using Microsoft.AspNetCore.Identity;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Wallet.Data.Models;
using Wallet.Services.Contracts;
using System.Drawing;
using System.Drawing.Imaging;


namespace Wallet.Services.Implementations
{
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IAccountService _accountService;
        private readonly IEmailSender _emailSender;

        public TwoFactorAuthService(IAccountService accountService, IEmailSender emailSender)
        {
            _accountService = accountService;
            _emailSender = emailSender;
        }

        public async Task<string> GenerateQrCodeUriAsync(AppUser user)
        {
            var key = await _accountService.GetOrGenerateAuthenticatorKeyAsync(user);
            return GenerateQrCodeUri(user.Email, key);
        }


        public async Task<byte[]> GenerateQrCodeImageAsync(AppUser user)
        {
            var key = await _accountService.GetOrGenerateAuthenticatorKeyAsync(user);
            var qrCodeUri = GenerateQrCodeUri(user.Email, key);

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(5);
            }
        }
     

        public async Task<bool> VerifyTwoFactorCodeAsync(AppUser user, string code)
        {
            return await _accountService.VerifyTwoFactorTokenAsync(user, code);
        }

        public async Task EnableTwoFactorAuthenticationAsync(AppUser user)
        {
            await _accountService.SetTwoFactorEnabledAsync(user, true);
        }

        public async Task DisableTwoFactorAuthenticationAsync(AppUser user)
        {
            await _accountService.SetTwoFactorEnabledAsync(user, false);
        }

        private string GenerateQrCodeUri(string email, string key)
        {
            return string.Format(
                "otpauth://totp/{0}?secret={1}&issuer={2}&digits=6",
                UrlEncoder.Default.Encode("YourAppName"),  // Replace with your application's name
                UrlEncoder.Default.Encode(key),
                UrlEncoder.Default.Encode(email));
        }
    }
}
