using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Models;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class AddFriendsAsyncTest : FriendsManagerTest
    {

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.Friendships.AnyAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(false));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.FirstOrDefaultAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult<FriendshipInvitation?>(new FriendshipInvitation()));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData("test","")]
        [InlineData("", "test")]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenUsersIdAreNullOrWhiteSpace(string user1Id, string user2Id)
        {
            // Act
            var result = await _friendsManager.AddFriendsAsync(user1Id, user2Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("SenderId and RecipientId cannot be null or empty.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIdAreTheSame()
        {
            // Act
            var result = await _friendsManager.AddFriendsAsync("test", "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Be("Sender ID and Recipient ID must be different.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenSenderUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleUser.Id, "recipientId") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("SenderUser was not found.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenRecipientUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("RecipientUser was not found.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenInvitationWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("The invitation was not found.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnConflictError_WhenFriendsAlreadyExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.Friendships.AnyAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.Conflict);
            error.Description.Should().Contain("This relationship has already exist.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test1"))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.AddFriendsAsync("test1", "Test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Description.Should().Contain("An internal server error occurred.");
        }
    }
}
