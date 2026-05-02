using Application.Common.Behaviors;
using Application.Common.Exceptions;
using Application.Common.Security;
using FakeItEasy;
using FluentAssertions;

namespace Application.UnitTests.Common.Behaviors
{
    public class UserRequestTests : AuthorizationBehaviorTestBase
    {
        public record UserRequest(Guid TargetUserId) : IRequireSameUser;

        private AuthorizationBehavior<UserRequest, Unit> CreateBehavior()
            => new(CurrentUser, ChatAccess, InvitationAccess, FriendshipAccess);

        [Fact]
        public async Task Should_Allow_When_UserIsTheSameAsTarget()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);

            var behavior = CreateBehavior();
            var request = new UserRequest(_sampleGuid1);

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Should_ThrowForbiddenException_When_UserIsNotTheSameAsTarget()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(_sampleGuid1);

            var behavior = CreateBehavior();
            var request = new UserRequest(_sampleGuid2);

            // Act
            Func<Task> act = async () => await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }
    }
}
