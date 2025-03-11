using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class GetFriendsAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IFriendsManager _friendsManager;
        private readonly UserAccount _sampleUser;

        public GetFriendsAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            _friendsManager = new FriendsManager(_userManager, _unitOfWork);
            _sampleUser = new UserAccount { UserName = "userLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab254" };
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            IEnumerable<Friendship> friendships = new List<Friendship>
            {
                new Friendship { Id = Guid.NewGuid(), User1Id = _sampleUser.Id, User2Id = "1", User2 = new UserAccount() {Id = "1", UserName = "userName1"} },
                new Friendship { Id = Guid.NewGuid(), User1Id = _sampleUser.Id, User2Id = "2", User2 = new UserAccount() {Id = "2", UserName = "userName2"} },
            };

            var expectedfriendsUserDtos = new List<FriendDto>
            {
                new FriendDto { UserName = "userName1", Id = "1" },
                new FriendDto { UserName = "userName2", Id = "2" }
            };

            A.CallTo(() => _unitOfWork.Friendships.WhereAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(friendships));

            // Act
            var result = await _friendsManager.GetFriendsAsync(_sampleUser.Id) as ResultT<List<FriendDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedfriendsUserDtos);
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnBadRequestError_WhenUserIdIsNullOrWhiteSpace()
        {
            // Act
            var result = await _friendsManager.GetFriendsAsync("") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("UserId cannot be null or empty.");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("testUser"))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.GetFriendsAsync("testUser") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("User was not found.");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnNotFound_WhenNotFoundAnyFriend()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            IEnumerable<Friendship> friendships = new List<Friendship>(); 

            A.CallTo(() => _unitOfWork.Friendships.WhereAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(friendships));

            // Act
            var result = await _friendsManager.GetFriendsAsync(_sampleUser.Id) as ResultT<List<FriendDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("No friends were found.");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test"))
                           .Throws(new Exception());
            // Act
            var result = await _friendsManager.GetFriendsAsync("test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }

    }
}
