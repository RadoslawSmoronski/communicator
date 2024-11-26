using Api.Data.IRepository;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Service;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Managers.TokenManagerTest
{
    public class RefreshAccessTokenAsyncTest
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly ITokenManager _tokenManager;

        public RefreshAccessTokenAsyncTest()
        {
            _refreshTokenRepository = A.Fake<IRefreshTokenRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var refreshToken = "cE1B4f9D-2d91-1c57-eFg4-36D4118BabAC";
            var user = new UserAccount { UserName = "TestLogin123", Id = "123" };

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(refreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(refreshToken))
                           .Returns(Task.FromResult(user.Id));

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult(user));

            // Act
            var result = await tokenManager.RefreshAccessTokenAsync(refreshToken) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnBadRequestError_WhenRefreshTokenIsNull()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);

            // Act
            var result = await tokenManager.RefreshAccessTokenAsync(null);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error!.Description.Should().Contain("Refresh token must not be null or empty.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnNotFoundError_WhenRefreshTokenDoesntExistsInDatabase()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var refreshToken = "cE1B4f9D-2d91-1c57-eFg4-36D4118BabAC";
            var user = new UserAccount { UserName = "TestLogin123", Id = "123" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
               .Returns(Task.FromResult(user));

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(refreshToken))
                           .Returns(Task.FromResult(false));

            // Act
            var result = await tokenManager.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.NotFound);
            error!.Description.Should().Contain("Refresh token not found.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnNotFoundError_WhenUserDoesntExistsInRefreshTokenDatabase()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var refreshToken = "cE1B4f9D-2d91-1c57-eFg4-36D4118BabAC";
            var user = new UserAccount { UserName = "TestLogin123", Id = "123" };

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(refreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(refreshToken))
                           .Returns(Task.FromResult<string>(null));

            // Act
            var result = await tokenManager.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.NotFound);
            error!.Description.Should().Contain("Refresh token record doesn't have user data or refresh token have been deleted.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnNotFoundError_WhenUserDoesntExists()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var refreshToken = "cE1B4f9D-2d91-1c57-eFg4-36D4118BabAC";
            var user = new UserAccount { UserName = "TestLogin123", Id = "123" };

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(refreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(refreshToken))
                           .Returns(Task.FromResult(user.Id));

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult<UserAccount>(null));

            // Act
            var result = await tokenManager.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.NotFound);
            error!.Description.Should().Contain("User form refresh token record doesn't exist.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnBadRequestError_WhenUserUserNameIsEmptyOrNull()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var refreshToken = "cE1B4f9D-2d91-1c57-eFg4-36D4118BabAC";
            var user = new UserAccount { UserName = "", Id = "123" };

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(refreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(refreshToken))
                           .Returns(Task.FromResult(user.Id));

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult<UserAccount>(user));

            // Act
            var result = await tokenManager.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error!.Description.Should().Contain("Username must not be null or empty.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnBadRequestError_WhenUserIdIsEmptyOrNull()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var refreshToken = "cE1B4f9D-2d91-1c57-eFg4-36D4118BabAC";
            var user = new UserAccount { UserName = "test", Id = "" };

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(refreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(refreshToken))
                           .Returns(Task.FromResult("123"));

            A.CallTo(() => _userManager.FindByIdAsync("123"))
                           .Returns(Task.FromResult<UserAccount>(user));

            // Act
            var result = await tokenManager.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error!.Description.Should().Contain("UserId must not be null or empty.");
        }
    }
}
