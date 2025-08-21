using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class GetFriendshipByFriendsIdAsyncTest : FriendsServiceTest
    {
        [Fact]
        public async Task GetFriendshipByFriendsIdAsync_ShouldReturnSuccess_WhenEverythingIsOk()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Friendships.FirstOrDefaultAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult<Friendship?>(_sampleFriendship));

            // Act
            var result = await _friendsManager.GetFriendshipByFriendsIdAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as ResultT<Friendship>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetFriendshipByFriendsIdAsync_ShouldReturnNotFoundError_WhenFriendshipDoesntExist()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Friendships.FirstOrDefaultAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult<Friendship?>(null));

            // Act
            var result = await _friendsManager.GetFriendshipByFriendsIdAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as ResultT<Friendship>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetFriendshipByFriendsIdAsync_ShouldReturnUnknow_WhenThrowException()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Friendships.FirstOrDefaultAsync(A<Expression<Func<Friendship, bool>>>._))
                .Throws(new Exception());

            // Act
            var result = await _friendsManager.GetFriendshipByFriendsIdAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as ResultT<Friendship>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}