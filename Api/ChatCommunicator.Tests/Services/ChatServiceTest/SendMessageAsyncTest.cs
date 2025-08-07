using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Chat;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.ChatManagerTest
{
    public class SendMessageAsyncTest : ChatServiceTest
    {
        [Fact]
        public async Task SendMessageAsyncTest_ShouldReturnMessage_WhenEverythingIsOk()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(_sampleConversation));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            // Act
            var result = await _chatManager.SendMessageAsync(_sampleUser1.Id, _sampleConversation.Id, "test") as ResultT<Message>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().BeOfType(typeof(Message));
        }

        [Theory]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, SAMPLE_STRING_GUID)]
        [InlineData(SAMPLE_STRING_GUID, SAMPLE_STRING_EMPTY_GUID)]
        public async Task SendMessageAsyncTest_ShouldReturnValidationError_WhenInputDataIsNotValid(string fakeUserId, string fakeConversationId)
        {
            // Act
            var result = await _chatManager.SendMessageAsync(Guid.Parse(fakeUserId), Guid.Parse(fakeConversationId), "test") as ResultT<Message>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task SendMessageAsyncTest_ShouldReturnNotFoundError_WhenConversationDoesntExist()
        {
            //Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));
  
            // Act
            var result = await _chatManager.SendMessageAsync(_sampleUser1.Id, _sampleConversation.Id, "test") as ResultT<Message>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
            error.Code.Should().Be("CONVERSATION_NOT_FOUND");
        }

        [Fact]
        public async Task SendMessageAsyncTest_ShouldReturnNotFoundError_WhenUserDoesntExist()
        {
            //Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _chatManager.SendMessageAsync(_sampleUser1.Id, _sampleConversation.Id, "test") as ResultT<Message>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
            error.Code.Should().Be("USER_NOT_FOUND");
        }

        [Fact]
        public async Task SendMessageAsync_ShouldReturnInternalServerErrorError()
        {
            // Arrange

            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Throws(new Exception());

            // Act
            var result = await _chatManager.SendMessageAsync(_sampleUser1.Id, _sampleConversation.Id, "test") as ResultT<Message>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
