using Api.Models.Chat;
using Api.Models.Dtos.Chat;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace Api.Tests.Managers.ChatManagerTest
{
    public class GetPagedMessagesAsyncTest : ChatManagerTest
    {
        [Fact]
        public async Task GetPagedMessagesAsync_ShouldReturnOk()
        {
            // Arranged

            var _extraSampleMessageList = _sampleMessagesList.Where(x => x.Id != _sampleMessage1.Id);

            A.CallTo(() => _unitOfWork.Messages.GetPagedMessagesFromMessageIdAsync(
                _sampleConversation.Id,
                _sampleMessage2.Id,
                10))
                .Returns(Task.FromResult<IEnumerable<Message>>(_extraSampleMessageList));

            var expectedMessageDtos = _mapper.Map<List<MessageDto>>(_extraSampleMessageList);

            // Act
                var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleMessage2.Id) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedMessageDtos);
        }

        [Fact]
        public async Task GetPagedMessagesFromMessageIdAsync_ShouldReturnOk_WhenThereAreNoMessages()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Messages.GetPagedMessagesFromMessageIdAsync(
                _sampleConversation.Id,
                _sampleMessage2.Id,
                10))
                .Returns(Task.FromResult<IEnumerable<Message>>(new List<Message>()));

            var expectedMessageDtos = new List<MessageDto>();

            // Act
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleMessage2.Id) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedMessageDtos);
        }

        [Fact]
        public async Task GetPagedMessagesFromMessageIdAsync_ShouldReturnNotFoundError_WhenConversionIdWasNotFound()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));

            // Act
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleMessage2.Id) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetPagedMessagesFromMessageIdAsync_ShouldReturnBadRequestError_WhenInputDataIsNotValid()
        {
            // Act
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(Guid.Empty, _sampleMessage2.Id) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task GetPagedMessagesFromMessageIdAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Messages.GetPagedMessagesFromMessageIdAsync(
                _sampleConversation.Id,
                _sampleMessage2.Id,
                10))
            .Throws(new Exception());

            // Act
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleMessage2.Id) as ResultT<List<MessageDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
