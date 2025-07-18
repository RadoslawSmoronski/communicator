using ChatCommunicator.Contracts;
using FakeItEasy;
using ChatCommunicator.Shared.Result;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.TokenServiceTest
{
    public class CreateRefreshTokenAsyncTest : TokenServiceTest
    {
        [Fact]
        public async Task CreateRefreshTokenAsync_ShouldReturnSuccess()
        {
            // Act
            var result = await _tokenService.CreateRefreshTokenAsync(_sampleUserId) as ResultT<Guid>;

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
            var result = await _tokenService.CreateRefreshTokenAsync(_sampleUserId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
