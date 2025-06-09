using ChatCommunicator.Contracts.Chat;
using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.ChatManagerTest
{
    public class GetChatsAsyncTest : ChatManagerTest
    {
        [Fact]
        public async Task GetChatsAsync_ShouldReturnOk()
        {
            // Arranged
            A.CallTo(() => _unitOfWork.Conversations.WhereAsync(
                A<Expression<Func<Conversation, bool>>>._,
                A<Expression<Func<Conversation, object>>[]>._))
                .Returns(Task.FromResult<IEnumerable<Conversation>>(_sampleConversationsList));

            var expectedChatDtos = _sampleConversationsList
                .Where(x => x.User1Id == _sampleUser1.Id || x.User2Id == _sampleUser1.Id)
                .Select(x =>
                {
                    var isUser1 = x.User1Id == _sampleUser1.Id;
                    var friend = isUser1 ? x.User2 : x.User1;

                    return new ChatDto
                    {
                        FriendId = friend!.Id,
                        FriendUserName = friend.UserName!,
                        ConversationId = x.Id,
                        LastMessageId = x.LastMessageId,
                        LastMessageContent = (x.LastMessage == null) ? null : x.LastMessage.Content,
                        IsFriendSenderMessage = x.LastMessage != null && x.LastMessage.SenderId == friend.Id,
                        LastMessageTimestamp = x.LastMessage?.Timestamp
                    };
                }).ToList();

            // Act
            var result = await _chatManager.GetChatsAsync(_sampleUser1.Id) as ResultT<List<ChatDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedChatDtos);
        }

        [Fact]
        public async Task GetChatsAsync_ShouldReturnBadRequestError_WhenInputDataIsNotValid()
        {
            // Act
            var result = await _chatManager.GetChatsAsync(Guid.Empty) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task GetChatsAsync_ShouldReturnInternalServerError()
        {
            // Arrange

            A.CallTo(() => _unitOfWork.Conversations.WhereAsync(
                A<Expression<Func<Conversation, bool>>>._,
                A<Expression<Func<Conversation, object>>[]>._))
                .Throws(new Exception());

            // Act
            var result = await _chatManager.GetChatsAsync(_sampleUser1.Id) as ResultT<List<ChatDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
