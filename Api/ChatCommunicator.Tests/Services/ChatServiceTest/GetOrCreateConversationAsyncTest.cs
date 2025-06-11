using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Chat;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Managers.ChatManagerTest
{
    public class GetOrCreateConversationAsyncTest : ChatServiceTest
    {
        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnOk_WhenConversationExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            //A.CallTo(() => _friendsManager.IsFriendsExistAsync(_sampleUser1.Id, _sampleUser2.Id))
            //    .Returns(Task.FromResult(true));

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
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id.ToString()))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser2));

            //A.CallTo(() => _friendsManager.IsFriendsExistAsync(_sampleUser1.Id, _sampleUser2.Id))
            //    .Returns(Task.FromResult(true));

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

        //[Theory]
        //[InlineData("test", "")]
        //[InlineData("", "test")]
        //public async Task GetOrCreateConversationAsync_ShouldReturnBadRequestError_WhenUsersIdAreNullOrWhiteSpace(string user1Id, string user2Id)
        //{
        //    // Act
        //    var result = await _chatManager.GetOrCreateConversationAsync(user1Id, user2Id) as Result;

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.IsSuccess.Should().BeFalse();
        //    result.Error.Should().NotBeNull();

        //    var error = result.Error! as Error;
        //    error.ErrorType.Should().Be(HttpErrorType.BadRequest);
        //    error.Description.Should().Contain("UserId or FriendId cannot be null or empty.");
        //}

        //[Fact]
        //public async Task GetOrCreateConversationAsync_ShouldReturnBadRequestError_WhenUserIdAndFriendIdAreTheSame()
        //{
        //    // Act
        //    var result = await _chatManager.GetOrCreateConversationAsync("test", "test") as Result;

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.IsSuccess.Should().BeFalse();
        //    result.Error.Should().NotBeNull();

        //    var error = result.Error!;
        //    error.ErrorType.Should().Be(HttpErrorType.BadRequest);
        //    error.Description.Should().Be("UserId and FriendId must be different.");
        //}

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnNotFoundError_WhenUserWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(_sampleUser1.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnNotFoundError_WhenFriendWasNotFound()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser2.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(_sampleUser1.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                           .Throws(new Exception());

            // Act
            var result = await _chatManager.GetOrCreateConversationAsync(_sampleUser1.Id, _sampleUser2.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}
