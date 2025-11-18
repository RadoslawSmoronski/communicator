using Application.Auth.Commands.RefreshAccessToken;
using Application.Common.Interfaces;
using Domain.Entities;
using FakeItEasy;
using FluentAssertions;
using Shared.Result;

namespace Application.UnitTests.Auth.Commands.RefreshAccessToken
{
    public class RefreshAccessTokenHandlerTests
    {
        private readonly ITokenService _tokenService = A.Fake<ITokenService>();
        private readonly RefreshAccessTokenHandler _handler;

        public RefreshAccessTokenHandlerTests()
        {
            _handler = new RefreshAccessTokenHandler(_tokenService);
        }

        [Fact]
        public async Task Handle_ShouldReturnReadModel_WhenRefreshSucceeds()
        {
            // Arrange
            var refreshTokenGuid = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new RefreshAccessTokenCommand(refreshTokenGuid);

            var refreshTokenObj = new RefreshToken
            {
                Token = refreshTokenGuid,
                UserId = userId
            };

            var updatedRefreshTokenObj = new RefreshToken
            {
                Token = refreshTokenGuid,
                UserId = userId
            };

            A.CallTo(() => _tokenService.GetRefreshTokenAsync(refreshTokenGuid))
                .Returns(Result<RefreshToken>.Success(refreshTokenObj));

            A.CallTo(() => _tokenService.UpdateRefreshToken(refreshTokenObj))
                .Returns(Result<RefreshToken>.Success(updatedRefreshTokenObj));

            A.CallTo(() => _tokenService.CreateAccessTokenAsync(userId))
                .Returns(Result<string>.Success("new-access-token"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AccessToken.Should().Be("new-access-token");
            result.Value.RefreshToken.Should().Be(refreshTokenGuid);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenGetRefreshTokenFails()
        {
            // Arrange
            var refreshTokenGuid = Guid.NewGuid();
            var command = new RefreshAccessTokenCommand(refreshTokenGuid);
            var error = Error.NotFound("RefreshTokenNotFound", "Token not found");

            A.CallTo(() => _tokenService.GetRefreshTokenAsync(refreshTokenGuid))
                .Returns(Result<RefreshToken>.Failure(error));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("RefreshTokenNotFound");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenUpdateRefreshTokenFails()
        {
            // Arrange
            var refreshTokenGuid = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new RefreshAccessTokenCommand(refreshTokenGuid);

            var refreshTokenObj = new RefreshToken
            {
                Token = refreshTokenGuid,
                UserId = userId
            };

            A.CallTo(() => _tokenService.GetRefreshTokenAsync(refreshTokenGuid))
                .Returns(Result<RefreshToken>.Success(refreshTokenObj));

            var error = Error.Failure("UpdateFailed", "Cannot update token");
            A.CallTo(() => _tokenService.UpdateRefreshToken(refreshTokenObj))
                .Returns(Result<RefreshToken>.Failure(error));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("UpdateFailed");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenCreateAccessTokenFails()
        {
            // Arrange
            var refreshTokenGuid = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var command = new RefreshAccessTokenCommand(refreshTokenGuid);

            var refreshTokenObj = new RefreshToken
            {
                Token = refreshTokenGuid,
                UserId = userId
            };

            A.CallTo(() => _tokenService.GetRefreshTokenAsync(refreshTokenGuid))
                .Returns(Result<RefreshToken>.Success(refreshTokenObj));

            A.CallTo(() => _tokenService.UpdateRefreshToken(refreshTokenObj))
                .Returns(Result<RefreshToken>.Success(refreshTokenObj));

            var error = Error.Failure("AccessTokenCreationFailed", "Cannot create access token");
            A.CallTo(() => _tokenService.CreateAccessTokenAsync(userId))
                .Returns(Result<string>.Failure(error));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("AccessTokenCreationFailed");
        }
    }
}
