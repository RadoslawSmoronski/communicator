//using Api.Data.IRepository;
//using Api.Managers;
//using Api.Managers.Interfaces;
//using Api.Models;
//using Api.Utilities.Result;
//using FakeItEasy;
//using FluentAssertions;
//using Microsoft.AspNetCore.Identity;

//namespace Api.Tests.Managers.FriendsManagerTest
//{
//    public class SendInviteAsyncTest
//    {
//        private readonly IFriendsRepository _friendsRepository;
//        private readonly UserManager<UserAccount> _userManager;

//        private readonly IFriendsManager _friendsManager;

//        private readonly UserAccount _sampleSenderUser;
//        private readonly UserAccount _sampleRecipientUser;

//        public SendInviteAsyncTest()
//        {

//            _friendsRepository = A.Fake<IFriendsRepository>();
//            _userManager = A.Fake<UserManager<UserAccount>>();

//            _friendsManager = new FriendsManager(_friendsRepository, _userManager);
//            _sampleSenderUser = new UserAccount { UserName = "senderUserLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab254" };
//            _sampleRecipientUser = new UserAccount { UserName = "recipientUserLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab255" };
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnOk()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
//                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

//            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id))
//               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

//            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(_sampleSenderUser.Id, _sampleRecipientUser.Id))
//                .Returns(Task.FromResult(false));

//            // Act
//            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeTrue();
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdIsNullOrWhiteSpace()
//        {
//            // Act
//            var result = await _friendsManager.SendInviteAsync("", "userId2") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
//            error.Code.Should().Be("SENDERID_IS_EMPTY");
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenRecipientIdIsNullOrWhiteSpace()
//        {
//            // Act
//            var result = await _friendsManager.SendInviteAsync("1", "") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
//            error.Code.Should().Be("RECIPIENTID_IS_EMPTY");
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIDAreTheSame()
//        {
//            // Act
//            var result = await _friendsManager.SendInviteAsync("1", "1") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
//            error.Code.Should().Be("SENDERID_AND_RECIPIENTID_ARE_THE_SAME");
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenSenderUserDoesNotExist()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
//                .Returns(Task.FromResult<UserAccount?>(null));


//            // Act
//            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, "recipientId") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(HttpErrorType.NotFound);
//            error.Code.Should().Be("SENDERUSER_NOT_FOUND");
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
//                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

//            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id))
//               .Returns(Task.FromResult<UserAccount?>(null));


//            // Act
//            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(HttpErrorType.NotFound);
//            error.Code.Should().Be("RECIPIENTUSER_NOT_FOUND");
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnConflictError_WhenInvitationExists()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
//                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

//            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id))
//               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

//            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(_sampleSenderUser.Id, _sampleRecipientUser.Id))
//                .Returns(Task.FromResult(true));

//            // Act
//            var result = await _friendsManager.SendInviteAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(HttpErrorType.Conflict);
//            error.Code.Should().Be("FRIENDS_INVITATION_EXISTS");
//        }

//        [Fact]
//        public async Task SendInviteAsync_ShouldReturnInternalServerError()
//        {
//            // Arrange
//            A.CallTo(() => _userManager.FindByIdAsync("test1"))
//                           .Throws(new Exception());

//            // Act
//            var result = await _friendsManager.SendInviteAsync("test1", "Test2") as Result;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
//            error.Code.Should().Be("INTERNAL_SERVER_ERROR");
//        }
//    }
//}
