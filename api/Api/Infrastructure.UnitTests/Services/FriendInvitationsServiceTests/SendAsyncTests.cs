using FluentAssertions;
using FakeItEasy;
using Domain.Entities;
using Infrastructure.Entities;

namespace Infrastructure.UnitTests.Services.FriendInvitationsServiceTests
{
    public class SendAsyncTests : FriendInvitationsServiceTestBase
    {
        [Fact]
        public async Task ReturnValidation_WhenSenderEqualsRecipient()
        {
            // Arrange
            var service = CreateService();
            
            // Act
            var result = await service.SendAsync(SampleSenderId, SampleSenderId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("FriendInvitation.SameUser");
        }

        [Fact]
        public async Task ReturnNotFound_WhenSenderNotFound()
        {
            // Arrange
            var service = CreateService();
            A.CallTo(() => UserManager.FindByIdAsync(SampleSenderId.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await service.SendAsync(SampleSenderId, SampleRecipientId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("FriendInvitation.SenderNotFound");
        }

        [Fact]
        public async Task SendAsync_Should_ReturnNotFound_WhenRecipientNotFound()
        {
            // Arrange
            var service = CreateService();
            A.CallTo(() => UserManager.FindByIdAsync(SampleSenderId.ToString()))
                .Returns(SampleSender);

            A.CallTo(() => UserManager.FindByIdAsync(SampleRecipientId.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await service.SendAsync(SampleSenderId, SampleRecipientId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("FriendInvitation.RecipientNotFound");
        }

        [Fact]
        public async Task SendAsync_Should_ReturnConflict_WhenInvitationAlreadyExists()
        {
            // Arrange
            var service = CreateService();
            A.CallTo(() => UserManager.FindByIdAsync(SampleSenderId.ToString()))
                .Returns(SampleSender);
            A.CallTo(() => UserManager.FindByIdAsync(SampleRecipientId.ToString()))
                .Returns(SampleRecipient);

            A.CallTo(() => UnitOfWork.FriendshipInvitations.IsExistAsync(SampleSenderId, SampleRecipientId))
                .Returns(Task.FromResult(true));

            // Act
            var result = await service.SendAsync(SampleSenderId, SampleRecipientId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("FriendInvitation.AlreadyExists");
        }

        [Fact]
        public async Task SendAsync_Should_SendInvitation_WhenAllValid()
        {
            // Arrange
            var service = CreateService();
            A.CallTo(() => UserManager.FindByIdAsync(SampleSenderId.ToString()))
                .Returns(SampleSender);
            A.CallTo(() => UserManager.FindByIdAsync(SampleRecipientId.ToString()))
                .Returns(SampleRecipient);

            A.CallTo(() => UnitOfWork.FriendshipInvitations.IsExistAsync(SampleSenderId, SampleRecipientId))
                .Returns(Task.FromResult(false));

            FriendshipInvitation? savedInvitation = null;
            A.CallTo(() => UnitOfWork.FriendshipInvitations.AddAsync(A<FriendshipInvitation>.Ignored))
                .Invokes((FriendshipInvitation inv) => savedInvitation = inv)
                .Returns(Task.CompletedTask);

            A.CallTo(() => UnitOfWork.SaveAsync()).Returns(Task.FromResult(1));

            // Act
            var result = await service.SendAsync(SampleSenderId, SampleRecipientId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            savedInvitation.Should().NotBeNull();
            savedInvitation!.SenderId.Should().Be(SampleSenderId);
            savedInvitation.RecipientId.Should().Be(SampleRecipientId);
        }
    }
}
