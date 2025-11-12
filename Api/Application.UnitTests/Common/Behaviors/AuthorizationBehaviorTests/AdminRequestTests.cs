using Application.Common.Behaviors;
using FakeItEasy;
using FluentAssertions;

namespace Application.UnitTests.Common.Behaviors
{
    public class AdminRequestTests : AuthorizationBehaviorTestBase
    {
        public record AdminRequest();

        private AuthorizationBehavior<AdminRequest, Unit> CreateBehavior()
            => new(CurrentUser, ChatAccess, InvitationAccess, FriendshipAccess);

        [Fact]
        public async Task Should_Allow_WhenUserIsInAdminRole()
        {
            // Arrange
            A.CallTo(() => CurrentUser.IsInRole("Admin")).Returns(true);

            var behavior = CreateBehavior();
            var request = new AdminRequest();

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
        }
    }
}
