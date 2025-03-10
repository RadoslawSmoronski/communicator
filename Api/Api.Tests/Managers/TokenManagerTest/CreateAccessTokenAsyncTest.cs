using Api.Managers.Interfaces;
using Api.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Api.Utilities.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Api.Managers;
using Api.Data.UnitOfWork;

namespace Api.Tests.Managers.TokenManagerTest
{
    public class CreateAccessTokenAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ITokenManager _tokenManager;
        private readonly UserAccount _sampleUserAccount;

        public CreateAccessTokenAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _unitOfWork = A.Fake<IUnitOfWork>();

            _tokenManager = new TokenManager(_configuration, _userManager, _unitOfWork);
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

        [Theory]
        [InlineData("login", "")]
        [InlineData("", "id")]
        public async Task CreateAccessTokenAsync_ShouldReturnBadRequestError_WhenDataIsNotValid(string login, string id)
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
            error.Description.Should().Be("User, Username, or User Id cannot be null or empty.");
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
            error.Description.Contains("User not found or username is invalid.");
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
