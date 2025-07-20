using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public class UploadAvatarAsyncTest : AccountManagerTest
    {
        private readonly IFormFile _fakeFile;

        public UploadAvatarAsyncTest()
        {
            _fakeFile = A.Fake<IFormFile>();
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userAvatarService.UploadAvatarAsync(_fakeFile))
                .Returns("Test");

            A.CallTo(() => _userManager.UpdateAsync(_sampleUser1))
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _accountManager.UploadAvatarAsync(_sampleUser1.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnValidationError_WhenUserIdIsEmpty()
        {
            //Act
            var result = await _accountManager.UploadAvatarAsync(Guid.Empty, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnConflict_WhenUserAlreadyHasAvatar()
        {
            // Arrange
            var json = JsonSerializer.Serialize(_sampleUser1);
            var localSampleUser1WithAvatar = JsonSerializer.Deserialize<UserAccount>(json);
            localSampleUser1WithAvatar!.AvatarUrl = "test";

            A.CallTo(() => _userManager.FindByIdAsync(localSampleUser1WithAvatar.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(localSampleUser1WithAvatar));


            //Act
            var result = await _accountManager.UploadAvatarAsync(localSampleUser1WithAvatar.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnUnauthorized_WhenUserWithUserIdDoesntExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            //Act
            var result = await _accountManager.UploadAvatarAsync(_sampleUser1.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnValidationError_WhenUploadAvatarAsyncReturnValidationError()
        {
            //Arrange
                A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                    .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

                A.CallTo(() => _userAvatarService.UploadAvatarAsync(_fakeFile))
                        .Returns(ResultT<string>.Failure(Error.Validation("test", "test")));

            //Act
            var result = await _accountManager.UploadAvatarAsync(_sampleUser1.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnUnknown_WhenThereIsException()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Throws(new Exception("test"));

            //Act
            var result = await _accountManager.UploadAvatarAsync(_sampleUser1.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}
