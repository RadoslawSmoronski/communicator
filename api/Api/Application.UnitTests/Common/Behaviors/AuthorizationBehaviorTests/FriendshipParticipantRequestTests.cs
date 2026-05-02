using Application.Common.Behaviors;
using Application.Common.Exceptions;
using Application.Common.Security;
using FakeItEasy;
using FluentAssertions;

namespace Application.UnitTests.Common.Behaviors
{
    public class FriendshipParticipantRequestTests : AuthorizationBehaviorTestBase
    {
        public record FriendshipParticipantRequest(Guid FriendshipId) : IRequireFriendshipParticipant;

        private AuthorizationBehavior<FriendshipParticipantRequest, Unit> CreateBehavior()
            => new(CurrentUser, ChatAccess, InvitationAccess, FriendshipAccess);

        [Fact]
        public async Task Should_Allow_When_UserIsFriendshipParticipant()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);
            A.CallTo(() => FriendshipAccess.IsParticipantAsync(_sampleGuid1, _sampleGuid2, CancellationToken.None))
                .Returns(true);

            var behavior = CreateBehavior();
            var request = new FriendshipParticipantRequest(_sampleGuid2);

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Should_ThrowForbidden_When_UserIsNotFriendshipParticipant()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);
            A.CallTo(() => FriendshipAccess.IsParticipantAsync(_sampleGuid1, _sampleGuid2, CancellationToken.None))
                .Returns(false);

            var behavior = CreateBehavior();
            var request = new FriendshipParticipantRequest(_sampleGuid2);

            // Act
            Func<Task> act = async () => await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }
    }
}
