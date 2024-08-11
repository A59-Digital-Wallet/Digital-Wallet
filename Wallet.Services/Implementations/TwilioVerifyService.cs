using Twilio;
using Twilio.Rest.Verify.V2.Service;
using Microsoft.Extensions.Configuration;

namespace Wallet.Services.Implementations
{
    public class TwilioVerifyService
    {
        private readonly IConfiguration _configuration;

        public TwilioVerifyService(IConfiguration configuration)
        {
            _configuration = configuration;
            TwilioClient.Init(_configuration["Twilio:AccountSID"], _configuration["Twilio:AuthToken"]);
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber)
        {
            var verification = await VerificationResource.CreateAsync(
                to: phoneNumber,
                channel: "sms",
                pathServiceSid: _configuration["Twilio:ServiceSID"]
            );

            return verification.Status == "pending";
        }

        public async Task<bool> VerifyCodeAsync(string phoneNumber, string code)
        {
            var verificationCheck = await VerificationCheckResource.CreateAsync(
                to: phoneNumber,
                code: code,
                pathServiceSid: _configuration["Twilio:ServiceSID"]
            );

            return verificationCheck.Status == "approved";
        }
    }
}
