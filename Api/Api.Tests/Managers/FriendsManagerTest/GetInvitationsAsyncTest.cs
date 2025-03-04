using Api.Data.IRepository;
using Api.Managers;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class GetInvitationsAsyncTest
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;

        private readonly FriendsManager _friendsManager;
        private readonly UserAccount _sampleUser;
        private readonly UserAccount _sampleUser2;

        public GetInvitationsAsyncTest()
        {

            _friendsRepository = A.Fake<IFriendsRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _friendsManager = new FriendsManager(_friendsRepository, _userManager);
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

            var friendshipInvitationDtos = new List<FriendshipInvitation>
            {
                new FriendshipInvitation{ Id = new Guid(),
                    SenderId = _sampleUser2.Id,
                    RecipientId = _sampleUser.Id,
                    SenderUser = _sampleUser2,
                    RecipientUser = _sampleUser,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var expectList = new List<FriendDto>
            {
                new FriendDto { UserName = _sampleUser2.UserName!, Id = _sampleUser2.Id }
            };

            A.CallTo(() => _friendsRepository.GetInvitationsAsync(_sampleUser.Id))
                .Returns(Task.FromResult(friendshipInvitationDtos));

            // Act
            var result = await _friendsManager.GetInvitationsAsync(_sampleUser.Id) as ResultT<List<FriendDto>>;

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
            error.Code.Should().Be("USERID_IS_EMPTY");
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
            error.Code.Should().Be("USERID_NOT_FOUND");
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnNotFound_WhenNotFoundAnyInvitations()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));


            A.CallTo(() => _friendsRepository.GetInvitationsAsync(_sampleUser.Id))
                .Returns(Task.FromResult(new List<FriendshipInvitation>()));

            // Act
            var result = await _friendsManager.GetInvitationsAsync(_sampleUser.Id) as ResultT<List<FriendDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Code.Should().Be("INVITITIES_NOT_FOUND");
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
