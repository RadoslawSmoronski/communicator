using Application.Common.Interfaces;
using Application.Users.Queries.GetChats;
using Domain.Entities;
using FakeItEasy;
using FluentAssertions;
using Shared.Result;

namespace Application.UnitTests.Users.Queries.GetChats
{
    public class GetChatsHandlerTests
    {
        private readonly IConversationService _conversationService;
        private readonly IFriendshipService _friendshipService;
        private readonly IUsersConnectionService _usersConnectionService;
        private readonly GetChatsHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _friendId = Guid.NewGuid();

        public GetChatsHandlerTests()
        {
            // Arrange - Fake dependencies
            _conversationService = A.Fake<IConversationService>();
            _friendshipService = A.Fake<IFriendshipService>();
            _usersConnectionService = A.Fake<IUsersConnectionService>();

            _handler = new GetChatsHandler(_conversationService, _friendshipService, _usersConnectionService);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoConversationsExist()
        {
            // Arrange
            A.CallTo(() => _conversationService.GetAll(_userId))
                .Returns(Result<List<Conversation>>.Success(new List<Conversation>()));

            var query = new GetChatsQuery(UserId: _userId, OnlyFriends: true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_ReturnError_WhenConversationServiceFails()
        {
            // Arrange
            var error = Shared.Result.Error.Failure("Conversation.Failure", "Failed");
            A.CallTo(() => _conversationService.GetAll(_userId))
                .Returns(error);
        
            var query = new GetChatsQuery(UserId: _userId, OnlyFriends: true);
        
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
        
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Conversation.Failure");
        }
        
        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoFriendsExist()
        {
            // Arrange
            var conversations = new List<Conversation> { new Conversation { User1Id = _userId, User2Id = _friendId } };
            A.CallTo(() => _conversationService.GetAll(_userId))
                .Returns(Result<List<Conversation>>.Success(conversations));
        
            A.CallTo(() => _friendshipService.GetUserFriendAsync(_userId))
                .Returns(Task.FromResult(Result<List<Friend>>.Success(new List<Friend>())));
        
            var query = new GetChatsQuery(UserId: _userId, OnlyFriends: true);
        
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
        
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
        
        [Fact]
        public async Task Handle_Should_ReturnChats_WhenConversationsAndFriendsExist()
        {
            // Arrange
            var friend = new Friend { Id = _friendId, UserName = "FriendUser", Email = "FriendUser@mail.com",FriendshipId =  Guid.NewGuid(), FriendshipCreatedAt =  DateTime.Now };
            var conversations = new List<Conversation>
            {
                new Conversation
                {
                    Id = Guid.NewGuid(),
                    User1Id = _userId,
                    User2Id = _friendId,
                    User1 = new User { Id = _userId, UserName = "Me", Email = "Me@mail.com"},
                    User2 = new User { Id = _friendId, UserName = "FriendUser", Email = "Me@mail.com"}
                }
            };
        
            A.CallTo(() => _conversationService.GetAll(_userId))
                .Returns(Result<List<Conversation>>.Success(conversations));
        
            A.CallTo(() => _friendshipService.GetUserFriendAsync(_userId))
                .Returns(Task.FromResult(Result<List<Friend>>.Success(new List<Friend> { friend })));
        
            A.CallTo(() => _usersConnectionService.GetOnlineUsersIdAsync())
                .Returns(Task.FromResult(new List<Guid> { _friendId }));
        
            var query = new GetChatsQuery(UserId: _userId, OnlyFriends: true);
        
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
        
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
        
            var chat = result.Value.First();
            chat.FriendId.Should().Be(_friendId);
            chat.IsFriendOnline.Should().BeTrue();
            chat.FriendUserName.Should().Be("FriendUser");
        }
        
        [Fact]
        public async Task Handle_Should_HandleFriendshipServiceFailure()
        {
            // Arrange
            var conversations = new List<Conversation> { new Conversation { User1Id = _userId, User2Id = _friendId } };
            A.CallTo(() => _conversationService.GetAll(_userId))
                .Returns(Result<List<Conversation>>.Success(conversations));
        
            var error = Shared.Result.Error.Failure("Friendship.Failure", "Failed");
            A.CallTo(() => _friendshipService.GetUserFriendAsync(_userId))
                .Returns(Task.FromResult<Result<List<Friend>>>(error));
        
            var query = new GetChatsQuery(UserId: _userId, OnlyFriends: true);
        
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
        
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("Friendship.Failure");
        }
    }
}
