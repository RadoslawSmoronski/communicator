using FakeItEasy;
using ChatCommunicator.Shared.Result;
using FluentAssertions;
using ChatCommunicator.Infrastructure.Models;

namespace ChatCommunicator.Tests.Services.TokenServiceTest
{
    public class CreateAccessTokenAsyncTest : TokenServiceTest
    {
        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _tokenService.CreateAccessTokenAsync(_sampleUserAccount) as ResultT<string>;

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
            var result = await _tokenService.CreateAccessTokenAsync(user);

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
            var result = await _tokenService.CreateAccessTokenAsync(_sampleUserAccount);

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
            var result = await _tokenService.CreateAccessTokenAsync(_sampleUserAccount);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
