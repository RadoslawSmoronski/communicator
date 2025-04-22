using Api.Models.Chat;
using Api.Models.Dtos.Chat;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace Api.Tests.Managers.ChatManagerTest
{
    public class GetMessagesAsyncTest : ChatManagerTest
    {
        [Fact]
        public async Task GetMessagesAsync_ShouldReturnOk()
        {
            // Arranged
            A.CallTo(() => _unitOfWork.Messages.WherePagedAsync(
                A<Expression<Func<Message, bool>>>._,
                A<Expression<Func<Message, DateTime>>>._,
                A<bool>._,
                A<int>._,
                A<int>._,
                A<Expression<Func<Message, object>>[]>._))
                .Returns(Task.FromResult<IEnumerable<Message>>(_sampleMessagesList));

            var expectedMessageDtos = _mapper.Map<List<MessageDto>>(_sampleMessagesList);

            // Act
            var result = await _chatManager.GetMessagesAsync(_sampleConversation.Id.ToString(), 1) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedMessageDtos);
        }

        [Fact]
        public async Task GetMessagesAsync_ShouldReturnOk_WhenThereAreNoMessages()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Messages.WhereAsync(A<Expression<Func<Message, bool>>>._))
                .Returns(Task.FromResult<IEnumerable<Message>>(new List<Message>()));

            var expectedMessageDtos = new List<MessageDto>();

            // Act
            var result = await _chatManager.GetMessagesAsync(_sampleConversation.Id.ToString(), 1) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedMessageDtos);
        }

        [Fact]
        public async Task GetMessagesAsync_ShouldReturnNotFoundError_WhenConversionIdWasNotFound()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));

            // Act
            var result = await _chatManager.GetMessagesAsync(_sampleConversation.Id.ToString(), 1) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
        }

        [Fact]
        public async Task GetMessagesAsync_ShouldReturnBadRequestError_WhenInputDataIsNotValid()
        {
            // Act
            var result = await _chatManager.GetMessagesAsync("", 1) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("ConversationId cannot be null or empty.");
        }

        [Fact]
        public async Task GetMessagesAsync_ShouldReturnInternalServerError()
        {
            // Arrange

            A.CallTo(() => _unitOfWork.Messages.WherePagedAsync(
                    A<Expression<Func<Message, bool>>>._,
                    A<Expression<Func<Message, DateTime>>>._,
                    A<bool>._,
                    A<int>._,
                    A<int>._,
                    A<Expression<Func<Message, object>>[]>._))
                .Throws(new Exception());

            // Act
            var result = await _chatManager.GetMessagesAsync(_sampleConversation.Id.ToString(), 1) as ResultT<List<MessageDto>>;

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
