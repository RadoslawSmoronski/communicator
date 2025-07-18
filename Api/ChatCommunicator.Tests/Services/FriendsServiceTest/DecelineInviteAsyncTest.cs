using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Application.Managers;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class DecelineInviteAsyncTest : FriendsServiceTest
    {
        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.AnyAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.FriendshipInvitations.FirstOrDefaultAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult<FriendshipInvitation?>(_sampleFriendshipInvitation));

            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(SAMPLE_STRING_GUID, SAMPLE_STRING_EMPTY_GUID)]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, SAMPLE_STRING_GUID)]
        public async Task DecelineInviteAsync_ShouldReturnBadRequestError_WhenSenderIdOrRecipientIdAreEmpty(string user1Id, string user2Id)
        {
            // Act
            var result = await _friendsManager.DecelineInviteAsync(Guid.Parse(user1Id), Guid.Parse(user2Id)) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIdAreTheSame()
        {
            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleUser.Id, _sampleUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnNotFoundError_WhenSenderUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnNotFoundError_WhenInvitationDoesntExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _unitOfWork.FriendshipInvitations
                .FirstOrDefaultAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult<FriendshipInvitation?>(null));

            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id.ToString()))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.DecelineInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}