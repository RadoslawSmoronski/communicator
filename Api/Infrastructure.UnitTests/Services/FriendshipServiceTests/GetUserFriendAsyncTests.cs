using FluentAssertions;
using FakeItEasy;
using Domain.Entities;
using Infrastructure.Entities;
using Shared.Result;

namespace Infrastructure.UnitTests.Services.FriendshipServiceTests
{
    public class GetUserFriendAsyncTests() : FriendshipServiceTestBase
    {
        [Fact]
        public async Task GetUserFriendAsync_Should_ReturnNotFound_WhenUserNotExist()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser1Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            var svc = CreateService();

            // Act
            var result = await svc.GetUserFriendAsync(SampleUser1Id);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Friendship.UserNotFound");
        }

        [Fact]
        public async Task GetUserFriendAsync_Should_ReturnEmptyList_WhenNoFriends()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser1Id.ToString()))
                .Returns(SampleUserAccount1);
            A.CallTo(() => UnitOfWork.Friendships.GetAllAsync(SampleUser1Id))
                .Returns(Task.FromResult(new List<Friendship>()));
            
            var svc = CreateService();

            // Act
            var result = await svc.GetUserFriendAsync(SampleUser1Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserFriendAsync_Should_ReturnMappedFriends_WhenFriendshipsExist()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser1Id.ToString()))
                .Returns(SampleUserAccount1);
        
            var friendship = new Friendship
            {
                Id = Guid.NewGuid(),
                User1Id = SampleUser1Id,
                User2Id = SampleUser2Id,
                User1 = new User() {Id = SampleUser1Id, UserName = SampleUserAccount1.UserName!, Email = SampleUserAccount1.Email!},
                User2 = new User() {Id = SampleUser2Id, UserName = SampleUserAccount2.UserName!, Email = SampleUserAccount2.Email!},
                CreatedAt = DateTime.UtcNow
            };
        
            A.CallTo(() => UnitOfWork.Friendships.GetAllAsync(SampleUser1Id))
                .Returns(Task.FromResult(new List<Friendship> { friendship }));
        
            var svc = CreateService();
        
            // Act
            var result = await svc.GetUserFriendAsync(SampleUser1Id);
        
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value![0].Id.Should().Be(SampleUser2Id);
            result.Value[0].FriendshipId.Should().Be(friendship.Id);
        }
    }
}
