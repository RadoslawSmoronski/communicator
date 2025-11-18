using System.Linq.Expressions;
using FluentAssertions;
using Domain.Entities;
using FakeItEasy;

namespace Infrastructure.UnitTests.Services.TokenServiceTests
{
    public class GetRefreshTokenTests : TokenServiceTestBase
    {
        [Fact]
        public async Task ReturnsToken_WhenExistsAndNotExpired()
        {
            // Arrange
            var rt = new RefreshToken { Token = SampleTokenId, UserId = SampleUserId, CreatedAt = DateTime.UtcNow };

            A.CallTo(() => UnitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                .Returns(Task.FromResult<RefreshToken?>(rt));

            var svc = CreateService();
            
            // Act
            var res = await svc.GetRefreshTokenAsync(SampleTokenId);

            // Assert
            res.IsSuccess.Should().BeTrue();
            ((RefreshToken)res.Value!).Token.Should().Be(SampleTokenId);
        }

        [Fact]
        public async Task ReturnsNotFound_WhenMissingOrExpired()
        {
            // Arrange
            A.CallTo(() => UnitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                .Returns(Task.FromResult<RefreshToken?>(null));

            var svc = CreateService();
            
            // Act
            var res = await svc.GetRefreshTokenAsync(SampleTokenId);

            // Assert
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("RefreshTokenNotFound");
        }

        [Fact]
        public async Task ReturnsFailure_OnException()
        {
            // Arrange
            A.CallTo(() => UnitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                .Throws(new Exception("db bad"));

            var svc = CreateService();
            
            // Act
            var res = await svc.GetRefreshTokenAsync(SampleTokenId);

            // Assert
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("RefreshTokenRetrievalFailed");
        }
    }
}