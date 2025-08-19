using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using System.Linq.Expressions;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public class DeleteAsyncTest : FriendsServiceTest
    {
        [Fact]
        public async Task DeleteAsyncTest_ShouldReturnSuccess_WhenEverythingIsOk()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Friendships.FirstOrDefaultAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult<Friendship?>(_sampleFriendship));

            // Act
            var result = await _friendsManager.DeleteAsync(_sampleFriendship.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsyncTest_ShouldReturnValidationError_WhenFriendshipIdIsEmptyGuid()
        {
            // Act
            var result = await _friendsManager.DeleteAsync(Guid.Parse(SAMPLE_STRING_EMPTY_GUID)) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task DeleteAsyncTest_ShouldReturnNotFoundError_WhenFriendshipDoesntExist()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Friendships.FirstOrDefaultAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult<Friendship?>(null));

            // Act
            var result = await _friendsManager.DeleteAsync(_sampleFriendship.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.NotFound);
        }

        [Fact]
        public async Task DeleteAsyncTest_ShouldReturnUnknow_WhenThrowException()
        {
            // Arrange
            A.CallTo(() => _unitOfWork.Friendships.FirstOrDefaultAsync(A<Expression<Func<Friendship, bool>>>._))
                .Throws(new Exception("test"));

            // Act
            var result = await _friendsManager.DeleteAsync(_sampleFriendship.Id) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(ErrorType.Unknown);
        }
    }
}