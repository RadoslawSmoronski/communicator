using Api.Data.IRepository;
using Api.Managers;
using Api.Models;
using Api.Utilities.Result;
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
    public class AddFriendsAsyncTest
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;

        public AddFriendsAsyncTest()
        {

            _friendsRepository = A.Fake<IFriendsRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var senderUser = new UserAccount { UserName = "senderUserLogin1", Id = "1" };
            var recipientUser = new UserAccount { UserName = "recipientUserLogin2", Id = "2" };

            A.CallTo(() => _userManager.FindByIdAsync(senderUser.Id))
                           .Returns(Task.FromResult(senderUser));

            A.CallTo(() => _userManager.FindByIdAsync(recipientUser.Id))
               .Returns(Task.FromResult(recipientUser));

            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(senderUser.Id, recipientUser.Id))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _friendsRepository.IsFriendsExists(senderUser.Id, recipientUser.Id))
                .Returns(Task.FromResult(false));

            A.CallTo(() => _friendsRepository.DeleteInviteAsync(senderUser, recipientUser))
                .Returns(Task.FromResult(Result.Success()));

            // Act
            var result = await friendsManager.AddFriendsAsync(senderUser.Id, recipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenSenderIdIsNullOrWhiteSpace()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            // Act
            var result = await friendsManager.AddFriendsAsync("", "userId2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("SENDERID_IS_EMPTY");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenRecipientIdIsNullOrWhiteSpace()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);


            // Act
            var result = await friendsManager.AddFriendsAsync("1", "") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("RECIPIENTID_IS_EMPTY");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIdAreTheSame()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);


            // Act
            var result = await friendsManager.AddFriendsAsync("test", "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("SENDERID_AND_RECIPIENTID_ARE_THE_SAME");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenSenderUserDoesNotExist()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var senderUser = new UserAccount { UserName = "senderUserLogin1", Id = "1" };

            A.CallTo(() => _userManager.FindByIdAsync(senderUser.Id))
                .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await friendsManager.AddFriendsAsync(senderUser.Id, "recipientId") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("SENDERUSER_NOT_FOUND");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var senderUser = new UserAccount { UserName = "senderUserLogin1", Id = "1" };
            var recipientUser = new UserAccount { UserName = "recipientUserLogin2", Id = "2" };

            A.CallTo(() => _userManager.FindByIdAsync(senderUser.Id))
                           .Returns(Task.FromResult(senderUser));

            A.CallTo(() => _userManager.FindByIdAsync(recipientUser.Id))
               .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await friendsManager.AddFriendsAsync(senderUser.Id, recipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("RECIPIENTUSER_NOT_FOUND");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenInvitationDoesntExists()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var senderUser = new UserAccount { UserName = "senderUserLogin1", Id = "1" };
            var recipientUser = new UserAccount { UserName = "recipientUserLogin2", Id = "2" };

            A.CallTo(() => _userManager.FindByIdAsync(senderUser.Id))
                           .Returns(Task.FromResult(senderUser));

            A.CallTo(() => _userManager.FindByIdAsync(recipientUser.Id))
               .Returns(Task.FromResult(recipientUser));

            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(senderUser.Id, recipientUser.Id))
                .Returns(Task.FromResult(false));

            // Act
            var result = await friendsManager.AddFriendsAsync(senderUser.Id, recipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("INVITATION_NOT_FOUND");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenFriendsAlreadyExists()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var senderUser = new UserAccount { UserName = "senderUserLogin1", Id = "1" };
            var recipientUser = new UserAccount { UserName = "recipientUserLogin2", Id = "2" };

            A.CallTo(() => _userManager.FindByIdAsync(senderUser.Id))
                           .Returns(Task.FromResult(senderUser));

            A.CallTo(() => _userManager.FindByIdAsync(recipientUser.Id))
               .Returns(Task.FromResult(recipientUser));

            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(senderUser.Id, recipientUser.Id))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _friendsRepository.IsFriendsExists(senderUser.Id, recipientUser.Id))
                .Returns(Task.FromResult(true));

            // Act
            var result = await friendsManager.AddFriendsAsync(senderUser.Id, recipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.Conflict);
            result.Error.Code.Should().Be("FRIENDS_ALREADY_EXISTS");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);


            A.CallTo(() => _userManager.FindByIdAsync("test1"))
                           .Throws(new Exception());

            // Act
            var result = await friendsManager.AddFriendsAsync("test1", "Test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            result.Error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }
    }
}
