using FluentAssertions;
using Infrastructure.Entities;
using System;
using System.Threading.Tasks;
using Domain.Entities;
using Xunit;
using FakeItEasy;

namespace Infrastructure.UnitTests.Services.TokenServiceTests
{
    public class UpdateRefreshTokenTests : TokenServiceTestBase
    {
        [Fact]
        public async Task UpdatesAndReturnsRefreshToken_WhenUserExists()
        {
            // Arrange
            var refreshToken = new RefreshToken { UserId = SampleUserId, Token = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddDays(-1) };

            A.CallTo(() => UserManager.FindByIdAsync(SampleUserId.ToString()))
                .Returns(Task.FromResult<UserAccount?>(SampleUserAccount));

            var svc = CreateService();
            
            // Act
            var res = await svc.UpdateRefreshToken(refreshToken);

            // Assert
            res.IsSuccess.Should().BeTrue();
            ((RefreshToken)res.Value!).Token.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var refreshToken = new RefreshToken { UserId = SampleUserId, Token = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddDays(-1) };
        
            A.CallTo(() => UserManager.FindByIdAsync(SampleUserId.ToString())).Returns(Task.FromResult<UserAccount?>(null));
        
            var svc = CreateService();
            
            // Act
            var res = await svc.UpdateRefreshToken(refreshToken);
        
            // Asserta
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("UserNotFound");
        }
        
        [Fact]
        public async Task ReturnsFailure_OnException()
        {
            // Arrange
            var refreshToken = new RefreshToken { UserId = SampleUserId, Token = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddDays(-1) };
        
            A.CallTo(() => UserManager.FindByIdAsync(SampleUserId.ToString())).Throws(new Exception("err"));
        
            var svc = CreateService();
            
            // Act
            var res = await svc.UpdateRefreshToken(refreshToken);
        
            // Assert
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("RefreshTokenCreationFailed");
        }
    }
}
