using Api.Data.IRepository;
using Api.Managers;
using Api.Models;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class AddFriendsAsyncTest
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;

        private readonly FriendsManager _friendsManager;
        private readonly UserAccount _sampleSenderUser;
        private readonly UserAccount _sampleRecipientUser;

        public AddFriendsAsyncTest()
        {

            _friendsRepository = A.Fake<IFriendsRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _friendsManager = new FriendsManager(_friendsRepository, _userManager);

            _sampleSenderUser = new UserAccount { UserName = "senderUserLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab254" };
            _sampleRecipientUser = new UserAccount { UserName = "recipientUserLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab255" };
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(_sampleSenderUser.Id, _sampleRecipientUser.Id))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _friendsRepository.IsFriendsExists(_sampleSenderUser.Id, _sampleRecipientUser.Id))
                .Returns(Task.FromResult(false));

            A.CallTo(() => _friendsRepository.DeleteInviteAsync(_sampleSenderUser, _sampleRecipientUser))
                .Returns(Task.FromResult(Result.Success()));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenSenderIdIsNullOrWhiteSpace()
        {
            // Act
            var result = await _friendsManager.AddFriendsAsync("", "userId2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("SenderId is required.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenRecipientIdIsNullOrWhiteSpace()
        {
            // Act
            var result = await _friendsManager.AddFriendsAsync("1", "") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("RecipientId is required.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnBadRequestError_WhenSenderIdAndRecipientIdAreTheSame()
        {
            // Act
            var result = await _friendsManager.AddFriendsAsync("test", "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Be("Sender ID and Recipient ID must be different.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenSenderUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
                .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, "recipientId") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("SenderUser doesn't exist.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenRecipientUserDoesNotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id))
               .Returns(Task.FromResult<UserAccount?>(null));


            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("RecipientUser doesn't exist.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenInvitationDoesntExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(_sampleSenderUser.Id, _sampleRecipientUser.Id))
                .Returns(Task.FromResult(false));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("The invitation doesn't exist.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnNotFoundError_WhenFriendsAlreadyExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleSenderUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleSenderUser));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleRecipientUser.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleRecipientUser));

            A.CallTo(() => _friendsRepository.IsFriendsInvitationExists(_sampleSenderUser.Id, _sampleRecipientUser.Id))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _friendsRepository.IsFriendsExists(_sampleSenderUser.Id, _sampleRecipientUser.Id))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _friendsManager.AddFriendsAsync(_sampleSenderUser.Id, _sampleRecipientUser.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.Conflict);
            error.Description.Should().Contain("This relationship already exists.");
        }

        [Fact]
        public async Task AddFriendsAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test1"))
                           .Throws(new Exception());

            // Act
            var result = await _friendsManager.AddFriendsAsync("test1", "Test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Description.Should().Contain("An internal server error occurred.");
        }
    }
}
