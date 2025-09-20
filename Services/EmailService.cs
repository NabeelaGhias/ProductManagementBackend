using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using ProductManagementSystem.Domain.Settings;

namespace ProductManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(new MailAddress(to));

                using var client = new SmtpClient
                {
                    Host = _emailSettings.SmtpServer,
                    Port = _emailSettings.SmtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    Timeout = 30000 // 30 seconds timeout
                };

                _logger.LogInformation($"Attempting to send email to {to} via {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}. Error: {ex.Message}");
                if (ex is SmtpException smtpEx)
                {
                    _logger.LogError($"SMTP Status Code: {smtpEx.StatusCode}, SMTP Response: {smtpEx.Message}");
                }
                throw new ApplicationException($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}