using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Api.Models.Dtos.Chat;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Runtime.Intrinsics.X86;

namespace Api.Tests.Managers.ChatManagerTest
{
    public class GetChatsAsyncTest : ChatManagerTest
    {
        [Fact]
        public async Task GetChatsAsync_ShouldReturnOk()
        {
            // Arranged
            A.CallTo(() => _unitOfWork.Conversations.WhereAsync(
                A<Expression<Func<Conversation, bool>>>._,
                A<Expression<Func<Conversation, object>>[]>._))
                .Returns(Task.FromResult<IEnumerable<Conversation>>(_sampleConversationsList));

            var expectedChatDtos = _sampleConversationsList
                .Where(x => x.User1Id == _sampleUser1.Id || x.User2Id == _sampleUser1.Id)
                .Select(x =>
                {
                    var isUser1 = x.User1Id == _sampleUser1.Id;
                    var friend = isUser1 ? x.User2 : x.User1;

                    return new ChatDto
                    {
                        FriendId = friend!.Id,
                        FriendUserName = friend.UserName!,
                        ConversationId = x.Id.ToString(),
                        LastMessageId = x.LastMessageId?.ToString(),
                        LastMessageContent = (x.LastMessage == null) ? null : x.LastMessage.Content,
                        IsFriendSenderMessage = x.LastMessage != null && x.LastMessage.SenderId == friend.Id,
                        LastMessageTimestamp = x.LastMessage?.Timestamp
                    };
                }).ToList();

            // Act
            var result = await _chatManager.GetChatsAsync(_sampleUser1.Id) as ResultT<List<ChatDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedChatDtos);
        }

        [Fact]
        public async Task GetChatsAsync_ShouldReturnBadRequestError_WhenInputDataIsNotValid()
        {
            // Act
            var result = await _chatManager.GetChatsAsync("") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error! as Error;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("UserId cannot be null or empty.");
        }

        [Fact]
        public async Task GetChatsAsync_ShouldReturnInternalServerError()
        {
            // Arrange

            A.CallTo(() => _unitOfWork.Conversations.WhereAsync(
                A<Expression<Func<Conversation, bool>>>._,
                A<Expression<Func<Conversation, object>>[]>._))
                .Throws(new Exception());

            // Act
            var result = await _chatManager.GetChatsAsync(_sampleUser1.Id) as ResultT<List<ChatDto>>;

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
