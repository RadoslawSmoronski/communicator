using Castle.Core.Logging;
using ChatCommunicator.API.Models;
using ChatCommunicator.Infrastructure.Services;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Runtime;

namespace ChatCommunicator.Tests.Services.EmailServiceTest
{
    public class SendAsyncTest
    {
        private readonly SmtpEmailService _fakeSmtpEmailService;
        private readonly ILogger<SmtpEmailService> _logger;

        public SendAsyncTest()
        {
            var fakeSettings = new SmtpEmailSettings
            {
                SmtpHost = "smtp.example.com",
                SmtpPort = 587,
                Username = "testuser@example.com",
                Password = "TestPassword123!",
                FromAddress = "noreply@example.com"
            };

            var options = A.Fake<IOptions<SmtpEmailSettings>>();
            A.CallTo(() => options.Value).Returns(fakeSettings);

            _logger = A.Fake<ILogger<SmtpEmailService>>();

            _fakeSmtpEmailService = new SmtpEmailService(options, _logger);
        }

        [Theory]
        [InlineData("", "Subject", "Body")]
        [InlineData("invalid-email", "Subject", "Body")]
        [InlineData("test@example.com", "", "Body")]
        [InlineData("test@example.com", "Subject", "")]
        public async Task SendAsync_ShouldReturnValidationError_WhenDataIsNotValid(string to, string subject, string body)
        {
            // Act
            var result = await _fakeSmtpEmailService.SendAsync(to, subject, body);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(ErrorType.Validation);
        }
    }
}