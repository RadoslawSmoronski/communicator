using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Controllers;
using Api.Data.IRepository;
using Api.Data.Repository;
using Api.Managers;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Responses;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class SendInviteAsyncTest
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;

        public SendInviteAsyncTest()
        {

            _friendsRepository = A.Fake<IFriendsRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnOk()
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
            var result = await friendsManager.SendInviteAsync(senderUser.Id, recipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdIsNullOrWhiteSpace()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            // Act
            var result = await friendsManager.SendInviteAsync("", "userId2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("SENDERID_IS_EMPTY");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenRecipientIdIsNullOrWhiteSpace()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);


            // Act
            var result = await friendsManager.SendInviteAsync("1", "") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("RECIPIENTID_IS_EMPTY");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIDAreTheSame()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);


            // Act
            var result = await friendsManager.SendInviteAsync("1", "1") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            result.Error.Code.Should().Be("SENDERID_AND_RECIPIENTID_ARE_THE_SAME");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenSenderUserDoesNotExist()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);

            var senderUser = new UserAccount { UserName = "senderUserLogin1", Id = "1" };

            A.CallTo(() => _userManager.FindByIdAsync(senderUser.Id))
                .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await friendsManager.SendInviteAsync(senderUser.Id, "recipientId") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("SENDERUSER_NOT_FOUND");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
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
            var result = await friendsManager.SendInviteAsync(senderUser.Id, recipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.NotFound);
            result.Error.Code.Should().Be("RECIPIENTUSER_NOT_FOUND");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnConflictError_WhenInvitationExists()
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

            // Act
            var result = await friendsManager.SendInviteAsync(senderUser.Id, recipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.Conflict);
            result.Error.Code.Should().Be("FRIENDS_INVITATION_EXISTS");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var friendsManager = new FriendsManager(_friendsRepository, _userManager);


            A.CallTo(() => _userManager.FindByIdAsync("test1"))
                           .Throws(new Exception());


            // Act
            var result = await friendsManager.SendInviteAsync("test1", "Test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            result.Error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }
    }
}
