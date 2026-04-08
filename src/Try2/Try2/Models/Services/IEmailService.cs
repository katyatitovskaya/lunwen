namespace Try2.Models.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendConfirmationCodeAsync(string toEmail, string code);
    }
}
