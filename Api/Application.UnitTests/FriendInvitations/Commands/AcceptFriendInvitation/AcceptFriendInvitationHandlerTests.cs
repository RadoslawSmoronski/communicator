using Application.FriendInvitations.Commands.AcceptFriendInvtation;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Domain.Entities;
using FluentAssertions;
using FakeItEasy;
using Shared.Result;

namespace Application.UnitTests.FriendInvitations.Commands.AcceptFriendInvitation
{
    public class AcceptFriendInvitationHandlerTests
    {
        private readonly IFriendInvitationsService _friendInvitationsService = A.Fake<IFriendInvitationsService>();
        private readonly IConversationService _conversationService = A.Fake<IConversationService>();
        private readonly IFriendshipService _friendshipService = A.Fake<IFriendshipService>();
        private readonly IUserService _userService = A.Fake<IUserService>();
        private readonly AcceptFriendInvitationHandler _handler;

        public AcceptFriendInvitationHandlerTests()
        {
            _handler = new AcceptFriendInvitationHandler(
                _friendInvitationsService,
                _conversationService,
                _friendshipService,
                _userService
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAcceptAsyncFails()
        {
            // Arrange
            var command = new AcceptFriendInvitationCommand(Guid.NewGuid());
            var error = Error.Failure("AcceptFail", "Cannot accept invitation");

            A.CallTo(() => _friendInvitationsService.AcceptAsync(command.InvitationId))
                .Returns(error);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Description.Should().Be("Cannot accept invitation");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenGetOrCreateConversationFails()
        {
            // Arrange
            var command = new AcceptFriendInvitationCommand(Guid.NewGuid());
            var invitation = new FriendshipInvitation
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };
            var conversationError = Error.Failure("ConversationFail", "Cannot create conversation");

            A.CallTo(() => _friendInvitationsService.AcceptAsync(command.InvitationId))
                .Returns(invitation);

            A.CallTo(() => _conversationService.GetOrCreateAsync(invitation.SenderId, invitation.RecipientId))
                .Returns(conversationError);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Description.Should().Be("Cannot create conversation");
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenAddFriendshipFails()
        {
            // Arrange
            var command = new AcceptFriendInvitationCommand(Guid.NewGuid());
            var invitation = new FriendshipInvitation
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };
            var conversation = new Conversation() { User1Id = Guid.NewGuid(), User2Id =  Guid.NewGuid() };
            var friendshipError = Error.Failure("FriendshipFail", "Cannot add friendship");
        
            A.CallTo(() => _friendInvitationsService.AcceptAsync(command.InvitationId))
                .Returns(invitation);
        
            A.CallTo(() => _conversationService.GetOrCreateAsync(invitation.SenderId, invitation.RecipientId))
                .Returns(Task.FromResult(Result<Conversation>.Success(conversation)));
        
            A.CallTo(() => _friendshipService.AddAsync(invitation.SenderId, invitation.RecipientId))
                .Returns(friendshipError);
        
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
        
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Description.Should().Be("Cannot add friendship");
        }

        [Fact]
        public async Task Handle_ShouldReturnReadModel_WhenAllStepsSucceed()
        {
            // Arrange
            var command = new AcceptFriendInvitationCommand(Guid.NewGuid());
            var invitation = new FriendshipInvitation
            {
                Id = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid()
            };
            var conversation = new Conversation() { User1Id = Guid.NewGuid(), User2Id =  Guid.NewGuid() };
            var friendshipId = Guid.NewGuid();
        
            A.CallTo(() => _friendInvitationsService.AcceptAsync(command.InvitationId))
                .Returns(invitation);
        
            A.CallTo(() => _conversationService.GetOrCreateAsync(invitation.SenderId, invitation.RecipientId))
                .Returns(Task.FromResult(Result<Conversation>.Success(conversation)));
        
            A.CallTo(() => _friendshipService.AddAsync(invitation.SenderId, invitation.RecipientId))
                .Returns(Task.FromResult(Result<Guid>.Success(friendshipId)));
        
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
        
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.ConversationId.Should().Be(conversation.Id);
            result.Value.FriendshipId.Should().Be(friendshipId);
        }
    }
}
