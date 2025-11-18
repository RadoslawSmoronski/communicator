using System.Linq.Expressions;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Entities;
using FakeItEasy;

namespace Infrastructure.UnitTests.Services.TokenServiceTests
{
    public class CreateRefreshTokenTests : TokenServiceTestBase
    {
        [Fact]
        public async Task CreatesNewToken_WhenNoExisting()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUserId.ToString()))
                .Returns(Task.FromResult<UserAccount?>(SampleUserAccount));
            
            A.CallTo(() => UnitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                .Returns(Task.FromResult<RefreshToken?>(null));
        
            var svc = CreateService();
            
            // Act
            var res = await svc.CreateRefreshTokenAsync(SampleUserId);
            
            // Assert
            res.IsSuccess.Should().BeTrue();
            ((Guid)res.Value!).Should().NotBe(Guid.Empty);
        }
        
        [Fact]
        public async Task UpdatesExistingToken_WhenFound()
        {
            // Arrange
            var existing = new RefreshToken
            {
                UserId = SampleUserId,
                Token = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            var oldToken = existing.Token;

            A.CallTo(() => UnitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                .Returns(Task.FromResult<RefreshToken?>(existing));

            var sut = CreateService();
            
            // Act
            var res = await sut.CreateRefreshTokenAsync(SampleUserId);
            
            // Assert
            res.IsSuccess.Should().BeTrue();
            existing.Token.Should().Be(res.Value);
            existing.Token.Should().NotBe(oldToken);
        }
        
        [Fact]
        public async Task ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUserId.ToString())).Returns(Task.FromResult<UserAccount?>(null));
        
            var svc = CreateService();
            
            // Act
            var res = await svc.CreateRefreshTokenAsync(SampleUserId);
        
            // Assert
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("UserNotFound");
        }
        
        [Fact]
        public async Task ReturnsFailure_OnException()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUserId.ToString())).Throws(new Exception("oops"));
        
            var svc = CreateService();
            
            // Act
            var res = await svc.CreateRefreshTokenAsync(SampleUserId);
        
            // Assert
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("RefreshTokenCreationFailed");
        }
    }
}
