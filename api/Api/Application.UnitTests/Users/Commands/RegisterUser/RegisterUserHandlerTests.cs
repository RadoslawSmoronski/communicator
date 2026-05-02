using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.Common.Settings;
using Application.Users.Commands.RegisterUser;
using AutoMapper;
using Domain.Entities;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;

namespace Application.UnitTests.Users.Commands.RegisterUser
{
    public class RegisterUserHandlerTests
    {
        private readonly IUserService _userService = A.Fake<IUserService>();
        private readonly IEmailService _emailService = A.Fake<IEmailService>();
        private readonly ILogger<RegisterUserHandler> _logger = A.Fake<ILogger<RegisterUserHandler>>();
        private readonly IMapper _mapper = A.Fake<IMapper>();
        private readonly IOptions<ConfirmEmailMessageSettings> _options;

        private readonly RegisterUserHandler _handler;

        public RegisterUserHandlerTests()
        {
            _options = Options.Create(new ConfirmEmailMessageSettings
            {
                Address = "https://example.com/confirm?",
                Content = "Please confirm your email: [address]",
                Title = "Confirm your email"
            });

            _handler = new RegisterUserHandler(_userService, _emailService, _logger, _options, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnReadModel_WhenRegistrationAndEmailSuccess()
        {
            // Arrange
            var command = new RegisterUserCommand("test@example.com", "TestUser", "Password123!");
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = command.Email, UserName =  command.Username, EmailConfirmed = false };

            A.CallTo(() => _userService.RegisterAsync(command.Email, command.Username, command.Password))
                .Returns(Result<User>.Success(user));

            var token = "confirmation-token";
            A.CallTo(() => _userService.GenerateEmailConfirmationTokenAsync(userId))
                .Returns(Result<string>.Success(token));

            A.CallTo(() => _mapper.Map<RegisterUserReadModel>(user))
                .Returns(new RegisterUserReadModel(userId, command.Email, command.Username));

            A.CallTo(() => _emailService.SendAsync(command.Email, _options.Value.Title, A<string>.Ignored))
                .Returns(Result.Success());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be(command.Email);
            result.Value.ConfirmToken.Should().Be(Uri.EscapeDataString(token));
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenRegistrationFails()
        {
            // Arrange
            var command = new RegisterUserCommand("test@example.com", "TestUser", "Password123!");
            var error = Error.Conflict("DuplicateEmail", "Email already exists");

            A.CallTo(() => _userService.RegisterAsync(command.Email, command.Username, command.Password))
                .Returns(Result<Domain.Entities.User>.Failure(error));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("DuplicateEmail");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenTokenGenerationFails()
        {
            // Arrange
            var command = new RegisterUserCommand("test@example.com", "TestUser", "Password123!");
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = command.Email, UserName =  command.Username, EmailConfirmed = false };

            A.CallTo(() => _userService.RegisterAsync(command.Email, command.Username, command.Password))
                .Returns(Result<User>.Success(user));

            var readModel = new RegisterUserReadModel(userId, command.Email, command.Username);
            A.CallTo(() => _mapper.Map<RegisterUserReadModel>(user)).Returns(readModel);

            var error = Error.Failure("EmailToken", "Failed to generate token");
            A.CallTo(() => _userService.GenerateEmailConfirmationTokenAsync(userId))
                .Returns(Result<string>.Failure(error));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("EmailToken");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenEmailSendFails()
        {
            // Arrange
            var command = new RegisterUserCommand("test@example.com", "TestUser", "Password123!");
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Email = command.Email, UserName =  command.Username, EmailConfirmed = false };

            A.CallTo(() => _userService.RegisterAsync(command.Email, command.Username, command.Password))
                .Returns(Result<User>.Success(user));

            var token = "confirmation-token";
            A.CallTo(() => _userService.GenerateEmailConfirmationTokenAsync(userId))
                .Returns(Result<string>.Success(token));

            var readModel = new RegisterUserReadModel(userId, command.Email, command.Username);
            A.CallTo(() => _mapper.Map<RegisterUserReadModel>(user)).Returns(readModel);

            var emailError = Error.Failure("EmailSend", "SMTP failed");
            A.CallTo(() => _emailService.SendAsync(command.Email, _options.Value.Title, A<string>.Ignored))
                .Returns(Result.Failure(emailError));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("EmailSend");
        }
    }
}
