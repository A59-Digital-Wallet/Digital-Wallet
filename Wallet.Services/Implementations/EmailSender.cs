﻿using SendGrid;
using SendGrid.Helpers.Mail;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmail(string subject, string toEmail, string username, string message)
        {
            var apiKey = "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"; //should put in secrets!!
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("kosetololbeast@gmail.com", "DigitalWallet");
            var to = new EmailAddress(toEmail, username);
            var plainTextContent = message;
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
