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
    public class SendInviteAsyncTest : FriendsManagerTest
    {

        [Fact]
        public async Task SendInviteAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "test")]
        [InlineData("test", "")]
        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdOrRecipientIdAreNullOrWhiteSpace(string senderId, string recipientId)
        {
            // Act
            var result = await _friendsManager.SendInviteAsync(senderId, recipientId) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("SenderId or RecipientId cannot be null or empty.");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIDAreTheSame()
        {
            // Act
            var result = await _friendsManager.SendInviteAsync("1", "1") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("Sender ID and Recipient ID must be different.");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenSenderUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleUser.Id, "recipientId") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("SenderUser was not found.");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("RecipientUser was not found");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnConflictError_WhenInvitationExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleUser.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.Conflict);
            error.Description.Should().Contain("An invitation has already exist.");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test1"))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.SendInviteAsync("test1", "Test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }
    }
}
