using ChatCommunicator.Application.Services;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace ChatCommunicator.Tests.Services.UserAvatarServiceTest
{
    public class UploadAvatarAsyncTest : UserAvatarServiceTest
    {
        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnSuccess_WhenFileIsValid()
        {
            // Arrange
            var image = CreateFakeImage(450, 450, 1 * 1024 * 1024);

            // Act
            var result = await _userAvatarService.UploadAvatarAsync(image) as ResultT<string>;

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Contain("avatar");
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnValidationError_WhenFileIsTooBig()
        {
            // Arrange
            var image = CreateFakeImage(450, 450, 6 * 1024 * 1024);

            // Act
            var result = await _userAvatarService.UploadAvatarAsync(image) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }
    }
}
