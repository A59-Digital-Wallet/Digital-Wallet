using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Contracts;

namespace Wallet.Services.Implementations
{
    public class EmailSender :  IEmailSender
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
