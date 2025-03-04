using Api.Managers.Interfaces;
using Api.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Api.Utilities.Result;
using FluentAssertions;
using Api.Data.IRepository;
using Api.Service;
using Microsoft.Extensions.Configuration;
using Api.Managers;

namespace Api.Tests.Managers.TokenManagerTest
{
    public class RemoveExpiredRefreshTokensAsyncTest
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;

        private readonly ITokenManager _tokenManager;

        public RemoveExpiredRefreshTokensAsyncTest()
        {
            _refreshTokenRepository = A.Fake<IRefreshTokenRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
        }


        [Fact]
        public async Task RemoveExpiredRefreshTokensAsync_ShouldReturnSuccess()
        {
            //Arrange
            A.CallTo(() => _refreshTokenRepository.RemoveExpiredRefreshTokensAsync()).Returns(2);

            // Act
            var result = await _tokenManager.RemoveExpiredRefreshTokensAsync() as ResultT<int>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(2);
        }

        [Fact]
        public async Task RemoveExpiredRefreshTokensAsync_ShouldReturnNotFound()
        {
            //Arrange
            A.CallTo(() => _refreshTokenRepository.RemoveExpiredRefreshTokensAsync()).Returns(0);

            // Act
            var result = await _tokenManager.RemoveExpiredRefreshTokensAsync() as ResultT<int>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Contains("Expired refresh tokens not found.");
        }

        [Fact]
        public async Task RemoveExpiredRefreshTokensAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _refreshTokenRepository.RemoveExpiredRefreshTokensAsync())
                .ThrowsAsync(new Exception());

            // Act
            var result = await _tokenManager.RemoveExpiredRefreshTokensAsync() as ResultT<int>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Description.Contains("An internal server error occurred.");
        }
    }
}
