using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Runtime.Intrinsics.X86;

namespace Api.Tests.Managers.ChatManagerTest
{
    public class DeleteConversationAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFriendsManager _friendsManager;

        private readonly IChatManager _chatManager;
        private readonly UserAccount _sampleUser1;
        private readonly UserAccount _sampleUser2;
        private readonly Conversation _sampleConversation;

        public DeleteConversationAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _friendsManager = A.Fake<IFriendsManager>();

            _chatManager = new ChatManager(_unitOfWork, _userManager, _friendsManager);
            _sampleUser1 = new UserAccount { UserName = "User1Login", Id = "c9fbf188-e309-48c9-811d-7d5be45ab254" };
            _sampleUser2 = new UserAccount { UserName = "User2Login", Id = "c9fbf188-e309-48c9-811d-7d5be45ab255" };

            _sampleConversation = new Conversation()
            {
                User1Id = _sampleUser1.Id,
                User2Id = _sampleUser2.Id,
                User1 = _sampleUser1,
                User2 = _sampleUser2
            };
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _friendsManager.IsFriendsExistAsync(_sampleUser1.Id, _sampleUser2.Id))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(_sampleConversation));


            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(_sampleUser1.Id,  _sampleUser2.Id) as ResultT<Conversation>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var value = result.Value!;
            value.Should().BeEquivalentTo(_sampleConversation);
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnOk_WhenConversationDoesnotExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            A.CallTo(() => _friendsManager.IsFriendsExistAsync(_sampleUser1.Id, _sampleUser2.Id))
                .Returns(Task.FromResult(true));

            A.CallTo(() => _unitOfWork.Conversations.FirstOrDefaultAsync(A<Expression<Func<Conversation, bool>>>._))
                .Returns(Task.FromResult<Conversation?>(null));


            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(_sampleUser1.Id, _sampleUser2.Id) as ResultT<Conversation>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var value = result.Value!;
            value.User1.Should().BeEquivalentTo(_sampleUser1);
            value.User2.Should().BeEquivalentTo(_sampleUser2);
        }

        [Theory]
        [InlineData("test", "")]
        [InlineData("", "test")]
        public async Task GetOrCreateConversationAsync_ShouldReturnBadRequestError_WhenUsersIdAreNullOrWhiteSpace(string user1Id, string user2Id)
        {
            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(user1Id, user2Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("UserId or FriendId cannot be null or empty.");
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnBadRequestError_WhenUserIdAndFriendIdAreTheSame()
        {
            // Act
            var result = await _chatManager.GetOrCreateConversationAsync("test", "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Be("UserId and FriendId must be different.");
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnNotFoundError_WhenUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(_sampleUser1.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("User was not found.");
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnNotFoundError_WhenFriendWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(_sampleUser1.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("Friend was not found.");
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test1"))
                           .Throws(new Exception());

            // Act
            var result = await _chatManager.GetOrCreateConversationAsync("test1", "Test2") as Result;

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
