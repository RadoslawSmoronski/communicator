using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public class ChangeUsernameAsyncTest : AccountManagerTest
    {
        private readonly string _newUsername;

        public ChangeUsernameAsyncTest()
        {
            _newUsername = "test";
        }

        [Fact]
        public async Task ChangeUsernameAsync_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.FindByNameAsync(_newUsername))
                .Returns(Task.FromResult<UserAccount?>(null));

            A.CallTo(() => _userManager.SetUserNameAsync(_sampleUser1, _newUsername))
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _accountManager.ChangeUsernameAsync(_sampleUser1.Id, _newUsername) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
        }

        [Theory]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, "test")]
        [InlineData(SAMPLE_STRING_GUID, null)]
        public async Task ChangeUsernameAsync_ShouldReturnValidationError_WhenUserIdOrNewUsernameAreNullOrEmpty(string userId, string newUsername)
        {
            //Act
            var result = await _accountManager.ChangeUsernameAsync(Guid.Parse(userId), newUsername) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task ChangeUsernameAsync_ShouldReturnUnauthorized_WhenUserWithUserIdDoesntExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            //Act
            var result = await _accountManager.ChangeUsernameAsync(_sampleUser1.Id, _newUsername) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        [Fact]
        public async Task ChangeUsernameAsync_ShouldReturnConflict_WhenUserWithNewUsernameIsExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.FindByNameAsync(_newUsername))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            //Act
            var result = await _accountManager.ChangeUsernameAsync(_sampleUser1.Id, _newUsername) as ResultT<string>;

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
            var result = await _accountManager.ChangeUsernameAsync(_sampleUser1.Id, _newUsername) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}
