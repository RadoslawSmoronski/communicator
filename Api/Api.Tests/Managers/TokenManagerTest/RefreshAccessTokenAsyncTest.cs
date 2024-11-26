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
        public async Task RefreshAccessTokenAsync_ShouldBadRequestError()
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
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldNotFoundError()
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
        }
    }
}
