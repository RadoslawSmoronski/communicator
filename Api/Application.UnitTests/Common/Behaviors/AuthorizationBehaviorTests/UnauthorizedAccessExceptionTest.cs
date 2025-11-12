using Application.Common.Behaviors;
using FakeItEasy;
using FluentAssertions;

namespace Application.UnitTests.Common.Behaviors
{
    public class UnauthorizedAccessExceptionTest : AuthorizationBehaviorTestBase
    {
        public record SomeRequest();

        [Fact]
        public async Task Should_ThrowUnauthorized_When_CurrentUserIdIsNull()
        {
            // Arrange
            A.CallTo(() => CurrentUser.Id).Returns(null);

            var behavior = new AuthorizationBehavior<SomeRequest, Unit>(
                CurrentUser, ChatAccess, InvitationAccess, FriendshipAccess);
            var request = new SomeRequest();

            // Act
            Func<Task> act = async () => await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
