using Application.Common.Behaviors;
using Application.Common.Security;
using FluentAssertions;

namespace Application.UnitTests.Common.Behaviors
{
    public class AnonymousRequestTests : AuthorizationBehaviorTestBase
    {
        public record AnonymousRequest() : IAllowAnonymous;

        private AuthorizationBehavior<AnonymousRequest, Unit> CreateBehavior()
            => new(CurrentUser, ChatAccess, InvitationAccess, FriendshipAccess);

        [Fact]
        public async Task Should_Allow_WhenUserIsAnonymous()
        {
            // Arrange
            var behavior = CreateBehavior();
            var request = new AnonymousRequest();

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
        }
    }
}
