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
    public class GetFriendsAsyncTest
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;

        public GetFriendsAsyncTest()
        {

            _friendsRepository = A.Fake<IFriendsRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var user = new UserAccount { UserName = "userName1", Id = "1" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult(user));

            var friendsUserDtos = new List<FriendDto>
            {
                new FriendDto { UserName = "userName1", Id = "1" },
                new FriendDto { UserName = "userName2", Id = "2" }
            };

            A.CallTo(() => _friendsRepository.GetFriendsAsync(user.Id))
                .Returns(Task.FromResult(friendsUserDtos));

            // Act
            var result = await friendsManager.GetFriendsAsync(user.Id) as ResultT<List<FriendDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(friendsUserDtos);
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnBadRequestError_WhenUserIdIsNullOrWhiteSpace()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            // Act
            var result = await friendsManager.GetFriendsAsync("") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error;
            error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("USERID_IS_EMPTY");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            A.CallTo(() => _userManager.FindByIdAsync("testUser"))
                           .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await friendsManager.GetFriendsAsync("testUser") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error;
            error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("USERID_NOT_FOUND");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnNotFound_WhenNotFoundAnyFriend()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var user = new UserAccount { UserName = "userName1", Id = "1" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult(user));


            A.CallTo(() => _friendsRepository.GetFriendsAsync(user.Id))
                .Returns(Task.FromResult(new List<FriendDto>()));

            // Act
            var result = await friendsManager.GetFriendsAsync(user.Id) as ResultT<List<FriendDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error;
            error.Should().NotBeNull();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("INVITITIES_NOT_FOUND");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            A.CallTo(() => _userManager.FindByIdAsync("test"))
                           .Throws(new Exception());
            // Act
            var result = await friendsManager.GetFriendsAsync("test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            result.Error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }

    }
}
