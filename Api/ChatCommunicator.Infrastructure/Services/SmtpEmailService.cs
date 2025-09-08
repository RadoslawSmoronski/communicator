using ChatCommunicator.API.Models;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ChatCommunicator.Infrastructure.Services
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
                _logger.LogWarning("Email send failed: Subject or body is empty. To: {To}", to);
                return Result.Failure(Error.Validation("Email.InvalidContent", "Email subject and body cannot be empty."));
            }

            if (string.IsNullOrEmpty(to) || !IsValidEmail(to))
            {
                _logger.LogWarning("Email send failed: Invalid recipient address. To: {To}", to);
                return Result.Failure(Error.Validation("Email.InvalidRecipient", "Recipient email address format is invalid."));
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

                _logger.LogInformation("Email sent successfully to {To}", to);
                return Result.Success();
            }
            catch (SmtpFailedRecipientException ex)
            {
                _logger.LogError(ex, "Email send failed for recipient: {FailedRecipient}", ex.FailedRecipient);
                return Result.Failure(Error.Validation("Email.FailedRecipient", $"Could not deliver email to recipient: {ex.FailedRecipient}."));
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error occurred while sending email. StatusCode: {StatusCode}", ex.StatusCode);
                return Result.Failure(Error.Failure("Email.SmtpError", $"SMTP error occurred while sending email. Status code: {ex.StatusCode}."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown error occurred while sending email.");
                return Result.Failure(Error.Unknown("Email.UnknownError", "An unexpected error occurred while sending the email."));
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
