using Api.Data.IRepository;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Service;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;


namespace Api.Tests.Managers.TokenManagerTest
{
    public class RefreshAccessTokenAsyncTest
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;

        private readonly ITokenManager _tokenManager;
        private readonly String _sampleRefreshToken;
        private readonly UserAccount _sampleUserAccount;

        public RefreshAccessTokenAsyncTest()
        {
            _refreshTokenRepository = A.Fake<IRefreshTokenRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            _sampleRefreshToken = new Guid().ToString();
            _sampleUserAccount = new UserAccount { UserName = "TestLogin123", Id = new Guid().ToString()};
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult<string?>(_sampleUserAccount.Id));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnBadRequestError_WhenRefreshTokenIsEmpty()
        {
            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync("");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("Refresh token cannot be null or empty.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnNotFoundError_WhenRefreshTokenDoesntExistsInDatabase()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult(false));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("Refresh token not found.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnNotFoundError_WhenUserDoesntExistsInRefreshTokenDatabase()
        {
            // Arrange
            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult<string?>(null));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("Refresh token record doesn't have user data, or the refresh token has been deleted.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnNotFoundError_WhenUserDoesntExists()
        {
            // Arrange
            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult<string?>(_sampleUserAccount.Id));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("User associated with the refresh token record doesn't exist.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnBadRequestError_WhenUserUserNameIsEmptyOrNull()
        {
            // Arrange
            var user = new UserAccount { UserName = "", Id = _sampleUserAccount.Id };

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult<string?>(user.Id));

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult<UserAccount?>(user));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("Username cannot be null or empty.");
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnBadRequestError_WhenUserIdIsEmptyOrNull()
        {
            // Arrange
            var user = new UserAccount { UserName = _sampleUserAccount.UserName, Id = "" };

            A.CallTo(() => _refreshTokenRepository.IsTokenValidAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _refreshTokenRepository.GetUserIdByRefreshTokenAsync(_sampleRefreshToken))
                           .Returns(Task.FromResult<string?>("123"));

            A.CallTo(() => _userManager.FindByIdAsync("123"))
                           .Returns(Task.FromResult<UserAccount?>(user));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("User Id cannot be null or empty.");
        }
    }
}
