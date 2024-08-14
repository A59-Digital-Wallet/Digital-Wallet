using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class VerifyEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly ConcurrentDictionary<string, string> _verificationCodes = new ConcurrentDictionary<string, string>();

        public VerifyEmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task<bool> SendVerificationCodeAsync(string email, string username)
        {
            // Generate a verification code
            var code = new Random().Next(100000, 999999).ToString();

            // Store the code with the email as the key
            _verificationCodes[email] = code;

            // Send the verification code via email
            var subject = "Your Verification Code";
            var message = $"Hello {username},\n\nYour verification code is: {code}\n\nPlease enter this code to complete your transaction.";
            await _emailSender.SendEmail(subject, email, username, message);

            return true; // Indicate that the code was sent successfully
        }

        public bool VerifyCode(string email, string code)
        {
            // Check if the provided code matches the one stored for the email
            if (_verificationCodes.TryGetValue(email, out var storedCode) && storedCode == code)
            {
                // Optionally, remove the code after verification
                _verificationCodes.TryRemove(email, out _);
                return true; // Verification successful
            }

            return false; // Verification failed
        }
    }
}
