using Application.Common.Behaviors;
using Application.Common.Exceptions;
using Application.Common.Security;
using FakeItEasy;
using FluentAssertions;

namespace Application.UnitTests.Common.Behaviors
{
    public class FriendInvitationRecipientRequestTests : AuthorizationBehaviorTestBase
    {
        public record FriendInvitationRecipientRequest(Guid FriendInvitationId) : IRequireFriendInvitationRecipient;

        private AuthorizationBehavior<FriendInvitationRecipientRequest, Unit> CreateBehavior()
            => new(CurrentUser, ChatAccess, InvitationAccess, FriendshipAccess);

        [Fact]
        public async Task Should_ThrowForbiddenException_When_UserIsNotRecipient()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);
            A.CallTo(() => InvitationAccess.IsRecipientAsync(_sampleGuid1, _sampleGuid2, CancellationToken.None))
                .Returns(false);

            var behavior = CreateBehavior();
            var request = new FriendInvitationRecipientRequest(_sampleGuid2);

            // Act
            Func<Task> act = async () => await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task Should_Allow_When_UserIsRecipient()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);
            A.CallTo(() => InvitationAccess.IsRecipientAsync(_sampleGuid1, _sampleGuid2, CancellationToken.None))
                .Returns(true);

            var behavior = CreateBehavior();
            var request = new FriendInvitationRecipientRequest(_sampleGuid2);

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
        }
    }
}
