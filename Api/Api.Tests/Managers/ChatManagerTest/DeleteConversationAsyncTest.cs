using Api.Models.Chat;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace Api.Tests.Managers.ChatManagerTest
{
    public class DeleteConversationAsyncTest : ChatManagerTest
    {
        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(_sampleConversation));

            // Act
            var result = await _chatManager.DeleteConversationAsync(_sampleConversation.Id.ToString()) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnBadRequestError_WhenInputDataIsNotValid()
        {
            // Act
            var result = await _chatManager.DeleteConversationAsync("") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("ConversationId cannot be null or empty.");
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnNotFoundError_WhenConversationWasNotFound()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));

            // Act
            var result = await _chatManager.DeleteConversationAsync("test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("Conversation was not found.");
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnInternalServerError()
        {
            // Arrange

            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Throws(new Exception());

            // Act
            var result = await _chatManager.DeleteConversationAsync(_sampleConversation.Id.ToString()) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Description.Should().Contain("An internal server error occurred.");
        }
    }
}
