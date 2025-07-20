using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class AddFriendsAsyncTest : FriendsServiceTest
    {
        [Fact]
        public async Task AddFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.Friendships.AnyAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(false));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.FirstOrDefaultAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult<FriendshipInvitation?>(_sampleFriendshipInvitation));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(SAMPLE_STRING_GUID, SAMPLE_STRING_EMPTY_GUID)]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, SAMPLE_STRING_GUID)]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenUsersIdAreNullOrWhiteSpace(string user1Id, string user2Id)
        {
            // Act
            var result = await _friendsManager.AddFriendsAsync(Guid.Parse(user1Id), Guid.Parse(user2Id)) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIdAreTheSame()
        {
            // Act
            var result = await _friendsManager.AddFriendsAsync(Guid.Empty, Guid.Empty) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenSenderUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenRecipientUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenInvitationWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnConflictError_WhenFriendsAlreadyExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.Friendships.AnyAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}