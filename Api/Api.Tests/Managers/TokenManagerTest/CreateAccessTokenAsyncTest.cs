using Api.Managers.Interfaces;
using Api.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Api.Utilities.Result;
using FluentAssertions;
using Api.Data.IRepository;
using Api.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting.Server;

namespace Api.Tests.Managers.TokenManagerTest
{
    public class CreateAccessTokenAsyncTest
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;

        private readonly ITokenManager _tokenManager;
        private readonly UserAccount _sampleUserAccount;

        public CreateAccessTokenAsyncTest()
        {
            _refreshTokenRepository = A.Fake<IRefreshTokenRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            _sampleUserAccount = new UserAccount { UserName = "TestLogin123", Id = new Guid().ToString() };
        }


        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(_sampleUserAccount) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnBadRequestError_WhenUserDoesntExist()
        {
            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(null);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Contains("User cannot be null.");
        }


        [Theory]
        [InlineData("login", "", "User Id cannot be null or empty.")]
        [InlineData("", "id", "Username cannot be null or empty.")]
        public async Task CreateAccessTokenAsync_ShouldReturnBadRequestError_WhenDataIsNotValid(string login, string id, string expectedDescription)
        {
            // Arrange
            var user = new UserAccount { UserName = login, Id = id };

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Contains(expectedDescription);
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnNotFoundErrorWhenUserNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(_sampleUserAccount);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Contains("The user doesn't exist.");
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .ThrowsAsync(new Exception());

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(_sampleUserAccount);

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
