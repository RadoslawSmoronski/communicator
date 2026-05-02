using Application.Common.Behaviors;
using Application.Common.Exceptions;
using Application.Common.Security;
using FakeItEasy;
using FluentAssertions;

namespace Application.UnitTests.Common.Behaviors
{
    public class ChatRequestTests : AuthorizationBehaviorTestBase
    {
        public record ChatRequest(Guid ChatId) : IRequireChatParticipant;

        private AuthorizationBehavior<ChatRequest, Unit> CreateBehavior()
            => new(CurrentUser, ChatAccess, InvitationAccess, FriendshipAccess);

        [Fact]
        public async Task Should_Allow_When_UserIsChatParticipant()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);
            A.CallTo(() => ChatAccess.IsParticipantAsync(_sampleGuid1, _sampleGuid2, CancellationToken.None))
                .Returns(true);

            var behavior = CreateBehavior();
            var request = new ChatRequest(_sampleGuid2);

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Should_ThrowForbiddenException_When_UserIsNotChatParticipant()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);

            var behavior = CreateBehavior();
            var request = new ChatRequest(_sampleGuid2);

            // Act
            Func<Task> act = async () => await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }
    }
}
