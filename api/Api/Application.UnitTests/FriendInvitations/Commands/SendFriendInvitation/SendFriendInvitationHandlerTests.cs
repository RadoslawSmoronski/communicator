using Application.FriendInvitations.Commands.SendFriendInvitation;
using Application.Common.Interfaces;
using FluentAssertions;
using FakeItEasy;
using Shared.Result;

namespace Application.UnitTests.FriendInvitations.Commands.SendFriendInvitation
{
    public class SendFriendInvitationHandlerTests
    {
        private readonly IFriendInvitationsService _friendInvitationsService = A.Fake<IFriendInvitationsService>();
        private readonly IFriendshipService _friendshipService = A.Fake<IFriendshipService>();
        private readonly SendFriendInvitationHandler _handler;

        public SendFriendInvitationHandlerTests()
        {
            _handler = new SendFriendInvitationHandler(_friendInvitationsService, _friendshipService);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenFriendshipAlreadyExists()
        {
            // Arrange
            var command = new SendFriendInvitationCommand(Guid.NewGuid(), Guid.NewGuid());

            A.CallTo(() => _friendshipService.IsExistAsync(command.SenderId, command.RecipientId))
                .Returns(Result.Success());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.Conflict);
            result.Error.Description.Should().Contain("Cannot send invitation");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenIsExistAsyncReturnsErrorOtherThanNotFound()
        {
            // Arrange
            var command = new SendFriendInvitationCommand(Guid.NewGuid(), Guid.NewGuid());
            var error = Error.Failure("TestError", "Something went wrong");

            A.CallTo(() => _friendshipService.IsExistAsync(command.SenderId, command.RecipientId))
                .Returns(error);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.Failure);
            result.Error.Description.Should().Be("Something went wrong");
        }

        [Fact]
        public async Task Handle_ShouldCallSendAsyncAndReturnValue_WhenFriendshipDoesNotExist()
        {
            // Arrange
            var command = new SendFriendInvitationCommand(Guid.NewGuid(), Guid.NewGuid());
            var invitationId = Guid.NewGuid();

            A.CallTo(() => _friendshipService.IsExistAsync(command.SenderId, command.RecipientId))
                .Returns(Error.NotFound("Friendship.NotExist", "Friendship does not exist"));

            A.CallTo(() => _friendInvitationsService.SendAsync(command.SenderId, command.RecipientId))
                .Returns(invitationId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(invitationId);

            A.CallTo(() => _friendInvitationsService.SendAsync(command.SenderId, command.RecipientId))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenSendAsyncFails()
        {
            // Arrange
            var command = new SendFriendInvitationCommand(Guid.NewGuid(), Guid.NewGuid());
            var error = Error.Failure("SendFailed", "Failed to send invitation");

            A.CallTo(() => _friendshipService.IsExistAsync(command.SenderId, command.RecipientId))
                .Returns(Error.NotFound("Friendship.NotExist", "Friendship does not exist"));

            A.CallTo(() => _friendInvitationsService.SendAsync(command.SenderId, command.RecipientId))
                .Returns(error);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Description.Should().Be("Failed to send invitation");
        }
    }
}
