using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Services.Contracts
{
    public interface IEmailSender
    {
        Task SendEmail(string subject, string toEmail, string username, string message);
    }
}
