using ChatCommunicator.Contracts.Chat;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.ChatManagerTest
{
    public class DeleteConversationAsyncTest : ChatServiceTest
    {
        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(_sampleConversation));

            // Act
            var result = await _chatManager.DeleteConversationAsync(_sampleConversation.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnBadRequestError_WhenInputDataIsNotValid()
        {
            // Act
            var result = await _chatManager.DeleteConversationAsync(Guid.Empty) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnNotFoundError_WhenConversationWasNotFound()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));

            // Act
            var result = await _chatManager.DeleteConversationAsync(_sampleConversation.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnInternalServerError()
        {
            // Arrange

            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Throws(new Exception());

            // Act
            var result = await _chatManager.DeleteConversationAsync(_sampleConversation.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
