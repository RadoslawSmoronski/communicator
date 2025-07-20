using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class GetInvitationsAsyncTest : FriendsServiceTest
    {
        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            IEnumerable<FriendshipInvitation> friendshipInvitations = new List<FriendshipInvitation>
            {
                new FriendshipInvitation{ Id = new Guid(),
                    SenderId = _sampleSenderUser.Id,
                    RecipientId = _sampleRecipientUser.Id,
                    SenderUser = _sampleSenderUser,
                    RecipientUser = _sampleRecipientUser,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var expectList = new List<SimpleUserDto>
            {
                new SimpleUserDto { userName = _sampleSenderUser.UserName!, Id = _sampleSenderUser.Id }
            };


            A.CallTo(() => _unitOfWork.FriendshipInvitations.WhereAsync(
                    A<Expression<Func<FriendshipInvitation, bool>>>._,
                    A<Expression<Func<FriendshipInvitation, object>>[]>._))
                    .Returns(Task.FromResult(friendshipInvitations));

            // Act
            var result = await _friendsManager.GetInvitationsAsync(_sampleUser.Id) as ResultT<List<SimpleUserDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var value = result.Value;
            value.Should().BeEquivalentTo(expectList);
        }

        [Fact]
        public async Task GetInvitationAsync_ShouldReturnBadRequestError_WhenUserIdIsEmpty()
        {
            // Act
            var result = await _friendsManager.GetInvitationsAsync(Guid.Empty) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task GetInvitationAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.GetInvitationsAsync(_sampleUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Throws(new Exception());
            // Act
            var result = await _friendsManager.GetInvitationsAsync(_sampleUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}
