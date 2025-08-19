using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class GetUserOnlineFriendsConnectionsIdAsyncTest : FriendsServiceTest
    {
        [Fact]
        public async Task GetUserOnlineFriendsConnectionsIdAsync_ShouldReturnValidationError_WhenDataIsEmptyGuid()
        {
            // Act
            var result = await _friendsManager.GetUserOnlineFriendsConnectionsIdAsync(Guid.Parse(SAMPLE_STRING_EMPTY_GUID));

            // Assert
            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task GetUserOnlineFriendsConnectionsIdAsync_ShouldReturnNotFoundError_WhenUserNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.GetUserOnlineFriendsConnectionsIdAsync(_sampleUser.Id);

            // Assert
            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetUserOnlineFriendsConnectionsIdAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.GetUserOnlineFriendsConnectionsIdAsync(_sampleUser.Id);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}