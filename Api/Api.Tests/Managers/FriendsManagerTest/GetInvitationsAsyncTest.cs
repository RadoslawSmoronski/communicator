using Api.Data.IRepository;
using Api.Managers;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using Castle.Components.DictionaryAdapter.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class GetInvitationsAsyncTest
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;

        public GetInvitationsAsyncTest()
        {

            _friendsRepository = A.Fake<IFriendsRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnOk()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var user = new UserAccount
            {
                Id = "c9fbf188-e309-48c9-811d-7d5be45ab255",
                UserName = "userName2"
            };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult(user));

            var senderUser = new UserAccount
            {
                Id = "c9fbf188-e309-48c9-811d-7d5be45ab254",
                UserName = "userName1"
            };

            var friendshipInvitationDtos = new List<FriendshipInvitation>
            {
                new FriendshipInvitation{ Id = new Guid(),
                    SenderId = senderUser.Id,
                    RecipientId = user.Id,
                    SenderUser = senderUser,
                    RecipientUser = user,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var expectList = new List<FriendDto>
            {
                new FriendDto { UserName = senderUser.UserName, Id = senderUser.Id }
            };

            A.CallTo(() => _friendsRepository.GetInvitationsAsync(user.Id))
                .Returns(Task.FromResult(friendshipInvitationDtos));

            // Act
            var result = await friendsManager.GetInvitationsAsync(user.Id) as ResultT<List<FriendDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectList);
        }

        [Fact]
        public async Task GetInvitationAsync_ShouldReturnBadRequestError_WhenUserIdIsNullOrWhiteSpace()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            // Act
            var result = await friendsManager.GetInvitationsAsync("") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error;
            error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("USERID_IS_EMPTY");
        }

        [Fact]
        public async Task GetInvitationAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            A.CallTo(() => _userManager.FindByIdAsync("testUser"))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await friendsManager.GetInvitationsAsync("testUser") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error;
            error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("USERID_NOT_FOUND");
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnNotFound_WhenNotFoundAnyInvitations()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var user = new UserAccount { UserName = "userName1", Id = "1" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult(user));


            A.CallTo(() => _friendsRepository.GetInvitationsAsync(user.Id))
                .Returns(Task.FromResult(new List<FriendshipInvitation>()));

            // Act
            var result = await friendsManager.GetInvitationsAsync(user.Id) as ResultT<List<FriendDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error;
            error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("INVITITIES_NOT_FOUND");
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            A.CallTo(() => _userManager.FindByIdAsync("test"))
                           .Throws(new Exception());
            // Act
            var result = await friendsManager.GetInvitationsAsync("test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            result.Error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }

    }
}
