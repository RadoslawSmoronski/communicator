using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class GetFriendsAsyncTest : FriendsServiceTest
    {
        [Fact]
        public async Task GetFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            IEnumerable<Friendship> friendships = new List<Friendship>
            {
                new Friendship { Id = Guid.NewGuid(), User1Id = _sampleUser.Id, User2Id = _sampleSenderUser.Id, User1 = _sampleUser, User2 = _sampleSenderUser},
                new Friendship { Id = Guid.NewGuid(), User1Id = _sampleUser.Id, User2Id = _sampleRecipientUser.Id, User1 = _sampleUser, User2 = _sampleRecipientUser },
            };


            var expectedfriendsUserDtos = new List<SimpleUserDto>
            {
                new SimpleUserDto { userName = _sampleSenderUser.UserName!, Id = _sampleSenderUser.Id },
                new SimpleUserDto { userName = _sampleRecipientUser.UserName!, Id = _sampleRecipientUser.Id }
            };

            A.CallTo(() => _unitOfWork.Friendships.WhereAsync(
                    A<Expression<Func<Friendship, bool>>>._,
                    A<Expression<Func<Friendship, object>>[]>._))
                .Returns(Task.FromResult(friendships));

            // Act
            var result = await _friendsManager.GetFriendsAsync(_sampleUser.Id) as ResultT<List<SimpleUserDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedfriendsUserDtos);
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnBadRequestError_WhenUserIdIsNullOrWhiteSpace()
        {
            // Act
            var result = await _friendsManager.GetFriendsAsync(Guid.Empty) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.GetFriendsAsync(_sampleUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Throws(new Exception());
            // Act
            var result = await _friendsManager.GetFriendsAsync(_sampleUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}