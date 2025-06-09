using ChatCommunicator.Data.UnitOfWork;
using ChatCommunicator.Managers;
using ChatCommunicator.Managers.Interfaces;
using ChatCommunicator.Models;
using ChatCommunicator.Models.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.FriendsManagerTest
{
    public class SendInviteAsyncTest : FriendsManagerTest
    {
        [Fact]
        public async Task SendInviteAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, SAMPLE_STRING_GUID)]
        [InlineData(SAMPLE_STRING_GUID, SAMPLE_STRING_EMPTY_GUID)]
        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdOrRecipientIdAreEmpty(string senderId, string recipientId)
        {
            // Act
            var result = await _friendsManager.SendInviteAsync(Guid.Parse(senderId), Guid.Parse(recipientId)) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIDAreTheSame()
        {
            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleUser.Id, _sampleUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenSenderUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnConflictError_WhenInvitationExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.SendInviteAsync(_sampleUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}