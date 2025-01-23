using Api.Data.IRepository;
using Api.Managers;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
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

            var user = new UserAccount { UserName = "userName1", Id = "1" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult(user));

            var invitationsUserDtos = new List<GetInvitationsUserDto>
            {
                new GetInvitationsUserDto { UserName = "userName1", Id = "1" },
                new GetInvitationsUserDto { UserName = "userName2", Id = "2" }
            };

            A.CallTo(() => _friendsRepository.GetInvitationsAsync(user.Id))
                .Returns(Task.FromResult(invitationsUserDtos));

            // Act
            var result = await friendsManager.GetInvitationsAsync(user.Id) as ResultT<List<GetInvitationsUserDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(invitationsUserDtos);
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
                .Returns(Task.FromResult(new List<GetInvitationsUserDto>()));

            // Act
            var result = await friendsManager.GetInvitationsAsync(user.Id) as ResultT<List<GetInvitationsUserDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error;
            error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("INVITITIES_NOT_FOUND");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnInternalServerError()
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
