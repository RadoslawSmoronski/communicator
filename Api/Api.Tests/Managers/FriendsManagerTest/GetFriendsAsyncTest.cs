//using Api.Data.IRepository;
//using Api.Managers;
//using Api.Managers.Interfaces;
//using Api.Models;
//using Api.Models.Dtos.Controllers.FriendsController;
//using Api.Utilities.Result;
//using FakeItEasy;
//using FluentAssertions;
//using Microsoft.AspNetCore.Identity;

//namespace Api.Tests.Managers.FriendsManagerTest
//{
//    public class GetFriendsAsyncTest
//    {
//        private readonly IFriendsRepository _friendsRepository;
//        private readonly UserManager<UserAccount> _userManager;

//        private readonly IFriendsManager _friendsManager;
//        private readonly UserAccount _sampleUser;

//        public GetFriendsAsyncTest()
//        {
//            _friendsRepository = A.Fake<IFriendsRepository>();
//            _userManager = A.Fake<UserManager<UserAccount>>();

//            _friendsManager = new FriendsManager(_friendsRepository, _userManager);
//            _sampleUser = new UserAccount { UserName = "userLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab254" };
//        }

//        [Fact]
//        public async Task GetFriendsAsync_ShouldReturnOk()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
//                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

//            var friendsUserDtos = new List<FriendDto>
//            {
//                new FriendDto { UserName = "userName1", Id = "1" },
//                new FriendDto { UserName = "userName2", Id = "2" }
//            };

//            A.CallTo(() => _friendsRepository.GetFriendsAsync(_sampleUser.Id))
//                .Returns(Task.FromResult(friendsUserDtos));

//            // Act
//            var result = await _friendsManager.GetFriendsAsync(_sampleUser.Id) as ResultT<List<FriendDto>>;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeTrue();
//            result.Value.Should().NotBeNull();
//            result.Value.Should().BeEquivalentTo(friendsUserDtos);
//        }

//        [Fact]
//        public async Task GetFriendsAsync_ShouldReturnBadRequestError_WhenUserIdIsNullOrWhiteSpace()
//        {
//            // Act
//            var result = await _friendsManager.GetFriendsAsync("") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error!;
//            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
//            error.Code.Should().Be("USERID_IS_EMPTY");
//        }

//        [Fact]
//        public async Task GetFriendsAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync("testUser"))
//                           .Returns(Task.FromResult<UserAccount?>(null));

//            // Act
//            var result = await _friendsManager.GetFriendsAsync("testUser") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error!;
//            error.ErrorType.Should().Be(HttpErrorType.NotFound);
//            error.Code.Should().Be("USERID_NOT_FOUND");
//        }

//        [Fact]
//        public async Task GetFriendsAsync_ShouldReturnNotFound_WhenNotFoundAnyFriend()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
//                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

//            A.CallTo(() => _friendsRepository.GetFriendsAsync(_sampleUser.Id))
//                .Returns(Task.FromResult(new List<FriendDto>()));

//            // Act
//            var result = await _friendsManager.GetFriendsAsync(_sampleUser.Id) as ResultT<List<FriendDto>>;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error!;
//            error.ErrorType.Should().Be(HttpErrorType.NotFound);
//            error.Code.Should().Be("INVITITIES_NOT_FOUND");
//        }

//        [Fact]
//        public async Task GetFriendsAsync_ShouldReturnInternalServerError()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync("test"))
//                           .Throws(new Exception());
//            // Act
//            var result = await _friendsManager.GetFriendsAsync("test") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error!;
//            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
//            error.Code.Should().Be("INTERNAL_SERVER_ERROR");
//        }

//    }
//}
