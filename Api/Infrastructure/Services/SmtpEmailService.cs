using Application.Interfaces;
using Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpEmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpEmailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<Result> SendAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Email subject or body is empty. Subject: '{Subject}', Body: '{Body}'", subject, body);
                return Error.Validation("Email.InvalidContent", "Subject and body must not be empty.");
            }

            if (string.IsNullOrEmpty(to))
            {
                _logger.LogWarning("Invalid recipient email address: '{To}'", to);
                return Error.Validation("Email.InvalidRecipient", "Recipient email address is invalid.");
            }

            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = true
                };

                var mail = new MailMessage(_settings.FromAddress, to, subject, body);
                await client.SendMailAsync(mail);

                _logger.LogInformation("Email sent successfully to '{To}' with subject '{Subject}'", to, subject);
                return Result.Success();
            }
            catch (SmtpFailedRecipientException ex)
            {
                _logger.LogError(ex, "Failed to deliver email to recipient: '{To}'", to);
                return Error.Validation("Email.DeliveryFailed", $"Failed to deliver email to recipient: {to}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while sending email to '{To}'", to);
                return Error.Failure("Email.SendError", $"An unexpected error occurred while sending email: {ex.Message}");
            }
        }
    }
}
