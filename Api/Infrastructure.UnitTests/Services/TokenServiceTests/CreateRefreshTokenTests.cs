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
        
            A.CallTo(() => UserManager.FindByIdAsync(SampleUserId.ToString()))
                .Returns(Task.FromResult<UserAccount?>(SampleUserAccount));
            
            A.CallTo(() => UnitOfWork.RefreshTokens.FirstOrDefaultAsync(A<Expression<Func<RefreshToken, bool>>>._))
                .Returns(Task.FromResult<RefreshToken?>(null));
        
            var svc = CreateService();
            var res = await svc.CreateRefreshTokenAsync(SampleUserId);
        
            res.IsSuccess.Should().BeTrue();
            ((Guid)res.Value!).Should().NotBe(Guid.Empty);
        }
        
        [Fact]
        public async Task UpdatesExistingToken_WhenFound()
        {
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
            var res = await sut.CreateRefreshTokenAsync(SampleUserId);

            res.IsSuccess.Should().BeTrue();
            existing.Token.Should().Be(res.Value);
            existing.Token.Should().NotBe(oldToken);

        }
        
        [Fact]
        public async Task ReturnsNotFound_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();
            A.CallTo(() => UserManager.FindByIdAsync(userId.ToString())).Returns(Task.FromResult<UserAccount?>(null));
        
            var svc = CreateService();
            var res = await svc.CreateRefreshTokenAsync(userId);
        
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("UserNotFound");
        }
        
        [Fact]
        public async Task ReturnsFailure_OnException()
        {
            var userId = Guid.NewGuid();
            A.CallTo(() => UserManager.FindByIdAsync(userId.ToString())).Throws(new Exception("oops"));
        
            var svc = CreateService();
            var res = await svc.CreateRefreshTokenAsync(userId);
        
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("RefreshTokenCreationFailed");
        }
    }
}
