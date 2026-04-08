// Try2.Models.Services.EmailService.cs
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Try2.Models.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

            using (var client = new SmtpClient(smtpServer, port))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(username, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendConfirmationCodeAsync(string toEmail, string code)
        {
            var subject = "Подтверждение email - Код подтверждения";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Подтверждение email адреса</h2>
                    <p>Ваш код подтверждения:</p>
                    <h1 style='color: #007bff; font-size: 32px; letter-spacing: 5px;'>{code}</h1>
                    <p>Код действителен в течение 15 минут.</p>
                    <p>Если вы не запрашивали этот код, проигнорируйте это письмо.</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}