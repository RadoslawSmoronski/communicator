using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Application.Services;


namespace ChatCommunicator.Tests.Services.TokenServiceTest
{
    public class RefreshAccessTokenAsyncTest : TokenServiceTest
    {

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.RefreshTokens.AnyAsync(A<Expression<Func<RefreshToken, bool>>>._))
                           .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                           .Returns(Task.FromResult<RefreshToken?>(new RefreshToken() {Token = Guid.NewGuid(), UserId = _sampleUserAccount.Id }));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _tokenService.RefreshAccessTokenAsync(_sampleRefreshToken) as ResultT<RefreshAccessTokenDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldReturnUnauthorizedError_WhenRefreshTokenDoesntExistsInDatabase()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            A.CallTo(() => _unitOfWork.RefreshTokens.AnyAsync(A<Expression<Func<RefreshToken, bool>>>._))
                           .Returns(Task.FromResult(false));

            // Act
            var result = await _tokenService.RefreshAccessTokenAsync(_sampleRefreshToken);

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
            var result = await _tokenService.RefreshAccessTokenAsync(_sampleRefreshToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
