namespace Wallet.Services.Contracts
{
    public interface IEmailSender
    {
        Task SendEmail(string subject, string toEmail, string username, string message);
    }
}
