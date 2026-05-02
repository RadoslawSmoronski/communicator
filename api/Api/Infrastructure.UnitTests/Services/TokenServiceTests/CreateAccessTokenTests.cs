using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Infrastructure.Entities;
using FakeItEasy;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.UnitTests.Services.TokenServiceTests
{
    public class CreateAccessTokenTests : TokenServiceTestBase
    {
        [Fact]
        public async Task ReturnsToken_WhenUserExists()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUserAccount.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(SampleUserAccount));
            
            var svc = CreateService();

            // Act
            var res = await svc.CreateAccessTokenAsync(SampleUserAccount.Id);

            // Assert
            res.IsSuccess.Should().BeTrue();
            var token = res.Value as string;
            token.Should().NotBeNullOrWhiteSpace();
            token!.Split('.').Length.Should().Be(3);
        }

        [Fact]
        public async Task ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            A.CallTo(() => UserManager.FindByIdAsync(userId.ToString())).Returns(Task.FromResult<UserAccount?>(null));

            var svc = CreateService();
            
            // Act
            var res = await svc.CreateAccessTokenAsync(userId);

            // Assert
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("UserNotFound");
        }

        [Fact]
        public async Task ReturnsFailure_OnException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            A.CallTo(() => UserManager.FindByIdAsync(userId.ToString())).Throws(new Exception("boom"));

            var svc = CreateService();
            
            // Act
            var res = await svc.CreateAccessTokenAsync(userId);

            
            // Assert
            res.IsSuccess.Should().BeFalse();
            res.Error!.Code.Should().Be("AccessTokenCreationFailed");
        }
    }
}