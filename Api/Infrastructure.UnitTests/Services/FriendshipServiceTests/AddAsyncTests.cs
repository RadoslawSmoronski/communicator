using FluentAssertions;
using FakeItEasy;
using Domain.Entities;
using Infrastructure.Entities;
using Shared.Result;

namespace Infrastructure.UnitTests.Services.FriendshipServiceTests
{
    public class AddAsyncTests : FriendshipServiceTestBase
    {
        [Fact]
        public async Task AddAsync_Should_ReturnValidation_WhenUserAddsSelf()
        {
            // Arrange
            var svc = CreateService();

            // Act
            var result = await svc.AddAsync(SampleUser1Id, SampleUser1Id);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.Validation);
            result.Error.Code.Should().Be("Friendship.SameUser");
        }

        [Fact]
        public async Task AddAsync_Should_ReturnNotFound_WhenUser1NotFound()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser1Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            var svc = CreateService();

            // Act
            var result = await svc.AddAsync(SampleUser1Id, SampleUser2Id);
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Friendship.User1NotFound");
        }

        [Fact]
        public async Task AddAsync_Should_ReturnNotFound_WhenUser2NotFound()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser1Id.ToString()))
                .Returns(SampleUserAccount1);
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser2Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            var svc = CreateService();

            // Act
            var result = await svc.AddAsync(SampleUser1Id, SampleUser2Id);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
            result.Error.Code.Should().Be("Friendship.User2NotFound");
        }

        [Fact]
        public async Task AddAsync_Should_ReturnConflict_WhenFriendshipExists()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser1Id.ToString())).Returns(SampleUserAccount1);
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser2Id.ToString())).Returns(SampleUserAccount2);
            A.CallTo(() => UnitOfWork.Friendships.IsExistAsync(SampleUser1Id, SampleUser2Id)).Returns(Task.FromResult(true));

            var svc = CreateService();

            // Act
            var result = await svc.AddAsync(SampleUser1Id, SampleUser2Id);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.Conflict);
            result.Error.Code.Should().Be("Friendship.AlreadyExists");
        }

        [Fact]
        public async Task AddAsync_Should_CreateFriendship_WhenValid()
        {
            // Arrange
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser1Id.ToString())).Returns(SampleUserAccount1);
            A.CallTo(() => UserManager.FindByIdAsync(SampleUser2Id.ToString())).Returns(SampleUserAccount2);
            A.CallTo(() => UnitOfWork.Friendships.IsExistAsync(SampleUser1Id, SampleUser2Id)).Returns(Task.FromResult(false));

            Friendship? createdFriendship = null;
            A.CallTo(() => UnitOfWork.Friendships.AddAsync(A<Friendship>.Ignored))
                .Invokes((Friendship f) => createdFriendship = f)
                .Returns(Task.CompletedTask);

            var svc = CreateService();

            // Act
            var result = await svc.AddAsync(SampleUser1Id, SampleUser2Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            createdFriendship.Should().NotBeNull();
            createdFriendship!.User1Id.Should().Be(SampleUser1Id);
            createdFriendship.User2Id.Should().Be(SampleUser2Id);
        }
    }
}
