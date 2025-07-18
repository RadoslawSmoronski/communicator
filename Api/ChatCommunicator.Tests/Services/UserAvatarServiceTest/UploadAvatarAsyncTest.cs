using ChatCommunicator.Shared.Result;
using FluentAssertions;

namespace ChatCommunicator.Tests.Services.UserAvatarServiceTest
{
    public class UploadAvatarAsyncTest : UserAvatarServiceTest
    {
        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnSuccess_WhenFileIsValid()
        {
            // Arrange
            var image = CreateFakeImageFormFile(450, 450, "fakeImage.png");

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
            var image = CreateFakeImageFormFile(2000, 2000, "fakeImage.bmp"); // ~ 11MB, which is larger than the 5MB limit
                
            // Act
            var result = await _userAvatarService.UploadAvatarAsync(image) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnValidationError_WhenFileIsTooLarge()
        {
            // Arrange
            var image = CreateFakeImageFormFile(503, 450, "fakeimage.png");

            // Act
            var result = await _userAvatarService.UploadAvatarAsync(image) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnValidationError_WhenFileIsTooSmall()
        {
            // Arrange
            var image = CreateFakeImageFormFile(90, 90, "fakeimage.png");

            // Act
            var result = await _userAvatarService.UploadAvatarAsync(image) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnValidationError_WhenFileIsNotValidFormat()
        {
            // Arrange
            var image = CreateFakeImageFormFile(450, 450, "fakeimage.tiff");

            // Act
            var result = await _userAvatarService.UploadAvatarAsync(image) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnValidationError_WhenFileIsEmpty()
        {
            // Act
            var result = await _userAvatarService.UploadAvatarAsync(null) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }
    }
}
