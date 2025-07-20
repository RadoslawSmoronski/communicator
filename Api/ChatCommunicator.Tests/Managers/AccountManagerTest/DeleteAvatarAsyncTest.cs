using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public class DeleteAvatarAsync : AccountManagerTest
    {
        private readonly IFormFile _fakeFile;

        public DeleteAvatarAsync()
        {
            _fakeFile = A.Fake<IFormFile>();
        }

        [Fact]
        public async Task DeleteAvatarAsync_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserWithAvatar.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUserWithAvatar));

            A.CallTo(() => _userManager.UpdateAsync(_sampleUserWithAvatar))
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _accountManager.DeleteAvatarAsync(_sampleUserWithAvatar.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAvatarAsync_ShouldReturnUnauthorized_WhenUserWithUserIdDoesntExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            //Act
            var result = await _accountManager.DeleteAvatarAsync(_sampleUser1.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        [Fact]
        public async Task DeleteAvatarAsync_ShouldReturnConflict_WhenUserHasNotAvatar()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            //Act
            var result = await _accountManager.DeleteAvatarAsync(_sampleUser1.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Conflict);
        }



        [Fact]
        public async Task ChangeUsernameAsync_ShouldReturnUnknown_WhenThereIsException()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Throws(new Exception());

            //Act
            var result = await _accountManager.DeleteAvatarAsync(_sampleUser1.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}
