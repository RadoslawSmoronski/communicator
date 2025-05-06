using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Service;
using Api.Service;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;


namespace Api.Tests.Managers.TokenManagerTest
{
    public class RefreshAccessTokenAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ITokenManager _tokenManager;
        private readonly String _sampleRefreshToken;
        private readonly UserAccount _sampleUserAccount;

        public RefreshAccessTokenAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _unitOfWork = A.Fake<IUnitOfWork>();

            _tokenManager = new TokenManager(_configuration, _userManager, _unitOfWork);
            _sampleRefreshToken = new Guid().ToString();
            _sampleUserAccount = new UserAccount { UserName = "TestLogin123", Id = new Guid().ToString() };
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.RefreshTokens.AnyAsync(A<Expression<Func<RefreshToken, bool>>>._))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                           .Returns(Task.FromResult<RefreshToken?>(new RefreshToken() {UserId = _sampleUserAccount.Id }));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken) as ResultT<RefreshAccessTokenDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnValidationError_WhenRefreshTokenIsEmpty()
        {
            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync("");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnUnauthorizedError_WhenRefreshTokenDoesntExistsInDatabase()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            A.CallTo(() => _unitOfWork.RefreshTokens.AnyAsync(A<Expression<Func<RefreshToken, bool>>>._))
                           .Returns(Task.FromResult(false));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnNotFoundError_WhenUserDoesntExistsInRefreshTokenDatabase()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.RefreshTokens.AnyAsync(A<Expression<Func<RefreshToken, bool>>>._))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                          .Returns(Task.FromResult<RefreshToken?>(null));

            // Act
            var result = await _tokenManager.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
