using ChatCommunicator.Managers.Interfaces;
using ChatCommunicator.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using ChatCommunicator.Shared.Result;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ChatCommunicator.Managers;
using ChatCommunicator.Data.UnitOfWork;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.TokenManagerTest
{
    public class CreateRefreshTokenAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ITokenManager _tokenManager;
        private readonly Guid _sampleUserId;

        public CreateRefreshTokenAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _unitOfWork = A.Fake<IUnitOfWork>();

            _tokenManager = new TokenManager(_configuration, _userManager, _unitOfWork);
            _sampleUserId = Guid.NewGuid();
        }


        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldReturnSuccess()
        {
            // Act
            var result = await _tokenManager.CreateRefreshTokenAsync(_sampleUserId) as ResultT<Guid>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldReturnUnknownError()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _tokenManager.CreateRefreshTokenAsync(_sampleUserId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
