using Application.Auth.Commands.LoginUser;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using AutoMapper;
using Domain.Entities;
using FakeItEasy;
using FluentAssertions;
using Shared.Result;

namespace Application.UnitTests.Auth.Commands.LoginUser
{
    public class LoginUserHandlerTests
    {
        private readonly IUserService _userService = A.Fake<IUserService>();
        private readonly ITokenService _tokenService = A.Fake<ITokenService>();
        private readonly LoginUserHandler _handler;
        
        private readonly User _sampleUser;

        public LoginUserHandlerTests()
        {
            _handler = new LoginUserHandler(_userService, _tokenService);
            
            _sampleUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "TestUser",
                Email = "TestUser@mail.com",
                AvatarUrl = "avatar.png"
            };
        }

        [Fact]
        public async Task Handle_ShouldReturnLoginUserReadModel_WhenLoginSucceeds()
        {
            // Arrange
            var command = new LoginUserCommand("test@example.com", "Password123!");

            A.CallTo(() => _userService.LoginAsync(command.Email, command.Password))
                .Returns(Result<User>.Success(_sampleUser));

            A.CallTo(() => _tokenService.CreateAccessTokenAsync(_sampleUser.Id))
                .Returns(Result<string>.Success("access-token"));

            A.CallTo(() => _tokenService.CreateRefreshTokenAsync(_sampleUser.Id))
                .Returns(Result<Guid>.Success(Guid.NewGuid()));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(_sampleUser.Id);
            result.Value.UserName.Should().Be("TestUser");
            result.Value.AvatarUrl.Should().Be("avatar.png");
            result.Value.AccessToken.Should().Be("access-token");
            result.Value.RefreshToken.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenLoginFails()
        {
            // Arrange
            var command = new LoginUserCommand("test@example.com", "wrongpass");
            var error = Error.Unauthorized("InvalidCredentials", "Invalid email or password.");

            A.CallTo(() => _userService.LoginAsync(command.Email, command.Password))
                .Returns(Result<User>.Failure(error));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("InvalidCredentials");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAccessTokenCreationFails()
        {
            // Arrange
            var command = new LoginUserCommand("test@example.com", "Password123!");

            A.CallTo(() => _userService.LoginAsync(command.Email, command.Password))
                .Returns(Result<User>.Success(_sampleUser));

            A.CallTo(() => _tokenService.CreateAccessTokenAsync(_sampleUser.Id))
                .Returns(Result<string>.Failure(Error.Failure("AccessTokenCreationFailed", "Failed")));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("AccessTokenCreationFailed");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenRefreshTokenCreationFails()
        {
            // Arrange
            var command = new LoginUserCommand("test@example.com", "Password123!");

            A.CallTo(() => _userService.LoginAsync(command.Email, command.Password))
                .Returns(Result<User>.Success(_sampleUser));

            A.CallTo(() => _tokenService.CreateAccessTokenAsync(_sampleUser.Id))
                .Returns(Result<string>.Success("access-token"));

            A.CallTo(() => _tokenService.CreateRefreshTokenAsync(_sampleUser.Id))
                .Returns(Result<Guid>.Failure(Error.Failure("RefreshTokenCreationFailed", "Failed")));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("RefreshTokenCreationFailed");
        }
    }
}
