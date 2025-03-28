using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class DecelineInviteAsyncTest : FriendsManagerTest
    {
        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.FirstOrDefaultAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult<FriendshipInvitation?>(new FriendshipInvitation()));

            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData("test", "")]
        [InlineData("", "test")]
        public async Task DecelineInviteAsync_ShouldReturnBadRequestError_WhenSenderIdOrRecipientIdAreNullOrWhiteSpace(string user1Id, string user2Id)
        {
            // Act
            var result = await _friendsManager.DecelineInviteAsync(user1Id, user2Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("SenderId and RecipientId cannot be null or empty.");
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIdAreTheSame()
        {
            // Act
            var result = await _friendsManager.DecelineInviteAsync("test", "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Be("SenderId and RecipientId must be different.");
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnNotFoundError_WhenSenderUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleUser.Id, "recipientId") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("SenderUser was not found.");
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("RecipientUser was not found.");
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnNotFoundError_WhenInvitationDoesntExists()
        {
            // Arrange
            var recipientUser = new UserAccount { UserName = "recipientUserLogin2", Id = "2" };

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _unitOfWork.FriendshipInvitations
                .FirstOrDefaultAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult<FriendshipInvitation?>(null));

            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("Invitation was not found.");
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test1"))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.DecelineInviteAsync("test1", "Test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }
    }
}
