using ChatCommunicator.Infrastructure.Models.Chat;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.ChatManagerTest
{
    public class SetAndGetUserLastReadMessage : ChatServiceTest
    {
        [Fact]
        public async Task SetAndGetUserLastFriendReadMessageAsync_ShouldReturnGuid_WhenEverythingIsOk()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Messages.GetUserLastFriendMessageAsync(_sampleConversation.Id, _sampleUser1.Id))
                .Returns(Task.FromResult<Message?>(_sampleMessage3));

            // Act
            var result = await _chatManager.SetAndGetUserLastFriendReadMessageAsync(_sampleUser1.Id, _sampleConversation.Id);

            // Assers
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().Be(_sampleMessage3.Id);
        }


        [Theory]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, SAMPLE_STRING_GUID)]
        [InlineData(SAMPLE_STRING_GUID, SAMPLE_STRING_EMPTY_GUID)]
        public async Task SetAndGetUserLastFriendReadMessageAsync_ShouldReturnValidationError_WhenDataAreNotValid(string userId, string conversationId)
        {
            // Act
            var result = await _chatManager.SetAndGetUserLastFriendReadMessageAsync(Guid.Parse(userId), Guid.Parse(conversationId));

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task SetAndGetUserLastFriendReadMessageAsync_ShouldReturnNotFound_WhenConversationWithConversationIdDoesntExist()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));

            // Act
            var result = await _chatManager.SetAndGetUserLastFriendReadMessageAsync(_sampleUser1.Id, _sampleConversation.Id);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
            error.Code.Should().Be("CONVERSATION_NOT_FOUND");
        }

        [Fact]
        public async Task GetPagedMessagesFromMessageIdAsync_ShouldReturnFoundError_WhenLastFriendMessageDoesntExist()
        {
            // Act

            A.CallTo(() => _unitOfWork.Messages.GetUserLastFriendMessageAsync(_sampleConversation.Id, _sampleUser1.Id))
                .Returns(Task.FromResult<Message?>(null));

            // Act
            var result = await _chatManager.SetAndGetUserLastFriendReadMessageAsync(_sampleUser1.Id, _sampleConversation.Id);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
            error.Code.Should().Be("LAST_FRIEND_MESSAGE_NOT_FOUND");
        }

        [Fact]
        public async Task SetAndGetUserLastFriendReadMessageAsync_ShouldReturnInternalServerError_WhenSetValueFailed()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Messages.GetUserLastFriendMessageAsync(_sampleConversation.Id, _sampleUser1.Id))
                .Throws(new Exception("test"));

            // Act
            var result = await _chatManager.SetAndGetUserLastFriendReadMessageAsync(_sampleUser1.Id, _sampleConversation.Id);

            //    // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
