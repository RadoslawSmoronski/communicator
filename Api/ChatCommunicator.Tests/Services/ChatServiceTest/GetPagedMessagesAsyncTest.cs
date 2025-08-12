using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Infrastructure.Models.Chat;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.ChatManagerTest
{
    public class GetPagedMessagesAsyncTest : ChatServiceTest
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

            A.CallTo(() => _usersConnectionService.GetUserConnectionsId(_sampleUser2.Id))
                .Returns(new List<string>());

            var expectedPagedMessagesDto = new PagedMessagesDto
            {
                Messages = _mapper.Map<List<MessageDto>>(_extraSampleMessageList),
                LastFriendReadMessageId = null
            };

            var expectedExtendedPagedMessagesDto = new ExtendedPagedMessagesDto
            {
                PagedMessagesDto = expectedPagedMessagesDto,
                RecipientConnectionsId = null
            };

            // Act
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleUser1.Id, _sampleMessage2.Id) as ResultT<ExtendedPagedMessagesDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedExtendedPagedMessagesDto);
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

            var expectedPagedMessagesDto = new PagedMessagesDto
            {
                Messages = new List<MessageDto>(),
                LastFriendReadMessageId = null
            };

            var expectedExtendedPagedMessagesDto = new ExtendedPagedMessagesDto
            {
                PagedMessagesDto = expectedPagedMessagesDto,
                RecipientConnectionsId = null
            };

            // Act
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleUser1.Id, _sampleMessage2.Id) as ResultT<ExtendedPagedMessagesDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedExtendedPagedMessagesDto);
        }

        [Fact]
        public async Task GetPagedMessagesFromMessageIdAsync_ShouldReturnNotFoundError_WhenConversionIdWasNotFound()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));

            // Act
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleUser1.Id, _sampleMessage2.Id) as ResultT<ExtendedPagedMessagesDto>;

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
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(Guid.Empty, _sampleUser1.Id, _sampleMessage2.Id) as ResultT<ExtendedPagedMessagesDto>;

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
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(_sampleConversation.Id, _sampleUser1.Id, _sampleMessage2.Id) as ResultT<ExtendedPagedMessagesDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
