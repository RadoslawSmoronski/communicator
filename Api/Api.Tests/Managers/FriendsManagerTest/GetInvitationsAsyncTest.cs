using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class GetInvitationsAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        private readonly FriendsManager _friendsManager;
        private readonly UserAccount _sampleUser;
        private readonly UserAccount _sampleUser2;

        public GetInvitationsAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            _friendsManager = new FriendsManager(_userManager, _unitOfWork);

            _sampleUser = new UserAccount
            {
                Id = "c9fbf188-e309-48c9-811d-7d5be45ab255",
                UserName = "userName"
            };

            _sampleUser2 = new UserAccount
            {
                Id = "d9fbf188-e309-48c9-811d-7d5be45ab255",
                UserName = "userName2"
            };
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            IEnumerable<FriendshipInvitation> friendshipInvitations = new List<FriendshipInvitation>
            {
                new FriendshipInvitation{ Id = new Guid(),
                    SenderId = _sampleUser2.Id,
                    RecipientId = _sampleUser.Id,
                    SenderUser = _sampleUser2,
                    RecipientUser = _sampleUser,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var expectList = new List<SimpleUserDto>
            {
                new SimpleUserDto { userName = _sampleUser2.UserName!, Id = _sampleUser2.Id }
            };


            A.CallTo(() => _unitOfWork.FriendshipInvitations.WhereAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(friendshipInvitations));

            // Act
            var result = await _friendsManager.GetInvitationsAsync(_sampleUser.Id) as ResultT<List<SimpleUserDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var value = result.Value;
            value.Should().BeEquivalentTo(expectList);
        }

        [Fact]
        public async Task GetInvitationAsync_ShouldReturnBadRequestError_WhenUserIdIsNullOrWhiteSpace()
        {
            // Act
            var result = await _friendsManager.GetInvitationsAsync("") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("UserId cannot be null or empty.");
        }

        [Fact]
        public async Task GetInvitationAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("testUser"))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.GetInvitationsAsync("testUser") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("User was not found.");
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnNotFound_WhenNotFoundAnyInvitations()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            IEnumerable<FriendshipInvitation> friendshipInvitations = new List<FriendshipInvitation>();

            A.CallTo(() => _unitOfWork.FriendshipInvitations.WhereAsync(A<Expression<Func<FriendshipInvitation, bool>>>._))
                .Returns(Task.FromResult(friendshipInvitations));

            // Act
            var result = await _friendsManager.GetInvitationsAsync(_sampleUser.Id) as ResultT<List<SimpleUserDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("Invitations were not found.");
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test"))
                           .Throws(new Exception());
            // Act
            var result = await _friendsManager.GetInvitationsAsync("test") as Result;

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
