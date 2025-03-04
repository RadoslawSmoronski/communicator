using Api.Managers.Interfaces;
using Api.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Api.Utilities.Result;
using FluentAssertions;
using Api.Data.IRepository;
using Api.Service;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Managers.TokenManagerTest
{
    public class CreateRefreshTokenAsyncTest
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;

        private readonly ITokenManager _tokenManager;
        private readonly String _sampleUserId;

        public CreateRefreshTokenAsyncTest()
        {
            _refreshTokenRepository = A.Fake<IRefreshTokenRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            _sampleUserId = new Guid().ToString();
        }


        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldReturnSuccess()
        {
            // Act
            var result = await _tokenManager.CreateRefreshTokenAsync(_sampleUserId) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            Guid.TryParse(result.Value as string, out Guid parsedGuid).Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateRefreshTokenAsync_ShouldReturnBadRequestError_WhenDataIsNotValid(string? testInput)
        {
            // Act
            var result = await _tokenManager.CreateRefreshTokenAsync(testInput);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Contains("User ID cannot be null or empty.");
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _refreshTokenRepository.GetRefreshTokenByUserIdAsync(_sampleUserId))
                           .ThrowsAsync(new Exception());

            // Act
            var result = await _tokenManager.CreateRefreshTokenAsync(_sampleUserId);

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
