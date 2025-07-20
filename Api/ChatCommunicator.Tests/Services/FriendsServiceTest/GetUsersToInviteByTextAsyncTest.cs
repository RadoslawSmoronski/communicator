using ChatCommunicator.Contracts.Dtos.Controllers.FriendsController;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;
using MockQueryable;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Friendship;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class GetUsersToInviteByTextAsync : FriendsServiceTest
    {
        [Fact]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            var fakeUsers = new List<UserAccount>
            {
                new UserAccount { Id = _sampleRecipientUser.Id, UserName = _sampleRecipientUser.UserName },
                new UserAccount { Id = _sampleSenderUser.Id, UserName = _sampleSenderUser.UserName },
            };

            A.CallTo(() => _userManager.Users)
                .Returns(fakeUsers.AsQueryable().BuildMock());

            A.CallTo(() => _unitOfWork.Friendships.AnyAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(false));

            var expectList = new List<UserToInviteDto>
            {
                new UserToInviteDto { Id = fakeUsers[0].Id, UserName = fakeUsers[0].UserName!, IsInvited = false },
                new UserToInviteDto { Id = fakeUsers[1].Id, UserName = fakeUsers[1].UserName!, IsInvited = false },
            };

            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(_sampleUser.Id, "User") as ResultT<List<UserToInviteDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var value = result.Value;
            value.Should().BeEquivalentTo(expectList);
        }

        [Theory]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, "test")]
        [InlineData(SAMPLE_STRING_GUID, "")]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnBadRequestError_WhenUserIdOrTextAreNullOrWhiteSpace(string fakeId, string fakeText)
        {
            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(Guid.Parse(fakeId), fakeText) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(_sampleUser.Id, "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id.ToString()))
                           .Throws(new Exception());
            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(_sampleUser.Id, "test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}