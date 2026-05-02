using Domain.Entities;
using FluentAssertions;
using FakeItEasy;
using Infrastructure.Entities;

namespace Infrastructure.UnitTests.Services.FriendInvitationsServiceTests
{
    public class GetUserInvitationsTests : FriendInvitationsServiceTestBase
    {
        [Fact]
        public async Task GetUserInvitations_Should_ReturnValidation_WhenUserIdEmpty()
        {
            // Arrange
            var service = CreateService();
            var emptyUserId = Guid.Empty;

            // Act
            var result = await service.GetUserInvitations(emptyUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("FriendInvitation.InvalidUserId");
        }

        [Fact]
        public async Task GetUserInvitations_Should_ReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var service = CreateService();
            A.CallTo(() => UserManager.FindByIdAsync(SampleSenderId.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await service.GetUserInvitations(SampleSenderId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("FriendInvitation.UserNotFound");
        }

        [Fact]
        public async Task GetUserInvitations_Should_ReturnInvitations_WhenUserExists()
        {
            // Arrange
            var service = CreateService();
            A.CallTo(() => UserManager.FindByIdAsync(SampleSenderId.ToString()))
                .Returns(SampleSender);

            var invitations = new List<FriendshipInvitation>
            {
                new FriendshipInvitation { Id = Guid.NewGuid(), SenderId = SampleSenderId, RecipientId = SampleRecipientId },
                new FriendshipInvitation { Id = Guid.NewGuid(), SenderId = SampleRecipientId, RecipientId = SampleSenderId }
            };

            A.CallTo(() => UnitOfWork.FriendshipInvitations.GetAllAsync(SampleSenderId))
                .Returns(Task.FromResult(invitations));

            // Act
            var result = await service.GetUserInvitations(SampleSenderId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value![0].SenderId.Should().Be(SampleSenderId);
        }

        [Fact]
        public async Task GetUserInvitations_Should_ReturnFailure_WhenExceptionThrown()
        {
            // Arrange
            var service = CreateService();
            A.CallTo(() => UserManager.FindByIdAsync(SampleSenderId.ToString()))
                .Throws(new Exception("DB failed"));

            // Act
            var result = await service.GetUserInvitations(SampleSenderId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("FriendInvitation.GetUserInvitations.Failure");
        }
    }
}
