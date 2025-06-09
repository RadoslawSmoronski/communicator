using ChatCommunicator.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using ChatCommunicator.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ChatCommunicator.API.Managers;
using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.API.Managers.Interfaces;

namespace ChatCommunicator.Tests.Managers.TokenManagerTest
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
            _sampleUserAccount = new UserAccount { UserName = "TestLogin123", Id = Guid.NewGuid() };
        }


        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(_sampleUserAccount) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData("login", "00000000-0000-0000-0000-000000000000")]
        [InlineData("", "d2719a18-7f24-4d57-85a2-2b42cc7d2827")]
        public async Task CreateAccessTokenAsync_ShouldReturnValidationError_WhenDataIsNotValid(string login, string id)
        {
            // Arrange
            var user = new UserAccount { UserName = login, Id = Guid.Parse(id) };

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnUnauthorizedErrorWhenUserNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(_sampleUserAccount);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnUnknownError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
                           .ThrowsAsync(new Exception());

            // Act
            var result = await _tokenManager.CreateAccessTokenAsync(_sampleUserAccount);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
