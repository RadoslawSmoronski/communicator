using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public class ChangeAvatarAsyncTest : AccountManagerTest
    {
        private readonly IFormFile _fakeFile;

        public ChangeAvatarAsyncTest()
        {
            _fakeFile = A.Fake<IFormFile>();
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            var json = JsonSerializer.Serialize(_sampleUser1);
            var localSampleUser1WithAvatar = JsonSerializer.Deserialize<UserAccount>(json);
            localSampleUser1WithAvatar!.AvatarUrl = "test";

            A.CallTo(() => _userManager.FindByIdAsync(localSampleUser1WithAvatar.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(localSampleUser1WithAvatar));

            A.CallTo(() => _userAvatarService.UploadAvatarAsync(_fakeFile))
                        .Returns("testChanged");

            A.CallTo(() => _userAvatarService.DeleteAvatar(localSampleUser1WithAvatar.AvatarUrl!))
                        .Returns(Result.Success());

            A.CallTo(() => _userManager.UpdateAsync(localSampleUser1WithAvatar))
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _accountManager.ChangeAvatarAsync(localSampleUser1WithAvatar.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            localSampleUser1WithAvatar.AvatarUrl.Should().Be("testChanged");
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnValidationError_WhenUserIdIsEmpty()
        {
            //Act
            var result = await _accountManager.ChangeAvatarAsync(Guid.Empty, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnUnauthorized_WhenUserWithUserIdDoesntExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            //Act
            var result = await _accountManager.ChangeAvatarAsync(_sampleUser1.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnConflict_WhenUserDoesntHaveAvatar()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));


            //Act
            var result = await _accountManager.ChangeAvatarAsync(_sampleUser1.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnUnknown_WhenDeleteFileFailed()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserWithAvatar.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUserWithAvatar));

            A.CallTo(() => _userAvatarService.UploadAvatarAsync(_fakeFile))
                        .Returns("testChanged");

            A.CallTo(() => _userAvatarService.DeleteAvatar(_sampleUserWithAvatar.AvatarUrl!))
                        .Returns(ResultT<string>.Failure(Error.Unknown("AVATAR_DELETE_FAILED", "test")));

            //Act
            var result = await _accountManager.ChangeAvatarAsync(_sampleUserWithAvatar.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
            result.Error!.Code.Should().Be("AVATAR_DELETE_FAILED");
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnUnknown_WhenUploadFileFailed()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserWithAvatar.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUserWithAvatar));

            A.CallTo(() => _userAvatarService.UploadAvatarAsync(_fakeFile))
                        .Returns(ResultT<string>.Failure(Error.Unknown("AVATAR_UPLOAD_FAILED", "test")));

            A.CallTo(() => _userAvatarService.DeleteAvatar(_sampleUserWithAvatar.AvatarUrl!))
                        .Returns(Result.Success());

            //Act
            var result = await _accountManager.ChangeAvatarAsync(_sampleUserWithAvatar.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
            result.Error!.Code.Should().Be("AVATAR_UPLOAD_FAILED");
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnUnknown_WhenUserUpdateFailed()
        {
            // Arrange
            A.CallTo(() => _userAvatarService.DeleteAvatar(_sampleUserWithAvatar.AvatarUrl!))
                        .Returns(Result.Success());

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserWithAvatar.Id.ToString()))
                        .Returns(Task.FromResult<UserAccount?>(_sampleUserWithAvatar));

            A.CallTo(() => _userAvatarService.UploadAvatarAsync(_fakeFile))
                        .Returns("testChanged");

            A.CallTo(() => _userAvatarService.DeleteAvatar(_sampleUserWithAvatar.AvatarUrl!))
                        .Returns(Result.Success());

            //Act
            var result = await _accountManager.ChangeAvatarAsync(_sampleUserWithAvatar.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
            result.Error!.Code.Should().Be("USER_UPDATE_FAILED");
        }

        [Fact]
        public async Task ChangeAvatarAsync_ShouldReturnUnknown_WhenThereIsException()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Throws(new Exception("test"));

            //Act
            var result = await _accountManager.ChangeAvatarAsync(_sampleUser1.Id, _fakeFile) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}
