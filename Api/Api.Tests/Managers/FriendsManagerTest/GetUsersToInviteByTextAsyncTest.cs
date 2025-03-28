using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using MockQueryable;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public class GetUsersToInviteByTextAsync : FriendsManagerTest
    {
        [Fact]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            var fakeUsers = new List<UserAccount>
            {
                new UserAccount { Id = "test", UserName = "test" },
                new UserAccount { Id = "test2", UserName = "test" },
                new UserAccount { Id = "test3", UserName = "test" },
            };

            A.CallTo(() => _userManager.Users)
                .Returns(fakeUsers.AsQueryable().BuildMock());

            A.CallTo(() => _unitOfWork.Friendships.AnyAsync(A<Expression<Func<Friendship, bool>>>._))
                .Returns(Task.FromResult(false));

            var expectList = new List<UserToInviteDto>
            {
                new UserToInviteDto { Id = fakeUsers[0].Id, userName = fakeUsers[0].UserName!, IsInvited = false },
                new UserToInviteDto { Id = fakeUsers[1].Id, userName = fakeUsers[1].UserName!, IsInvited = false },
                new UserToInviteDto { Id = fakeUsers[2].Id, userName = fakeUsers[2].UserName!, IsInvited = false }
            };

            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(_sampleUser.Id, "test") as ResultT<List<UserToInviteDto>>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            var value = result.Value;
            value.Should().BeEquivalentTo(expectList);
        }

        [Theory]
        [InlineData("", "test")]
        [InlineData("test", "")]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnBadRequestError_WhenUserIdOrTextAreNullOrWhiteSpace(string fakeId, string fakeText)
        {
            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(fakeId, fakeText) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.BadRequest);
            error.Description.Should().Contain("UserId or text cannot be null or empty.");
        }

        [Fact]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(_sampleUser.Id, "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("User was not found.");
        }

        [Fact]
        public async Task TaskGetUsersToInviteByTextAsync_ShouldReturnNotFound_WhenNotFoundAnyUser()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _userManager.Users)
                .Returns(new List<UserAccount>().AsQueryable().BuildMock());

            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync(_sampleUser.Id, "test") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.NotFound);
            error.Description.Should().Contain("Users to invite were not found.");
        }

        [Fact]
        public async Task GetUsersToInviteByTextAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync("test"))
                           .Throws(new Exception());
            // Act
            var result = await _friendsManager.GetUsersToInviteByTextAsync("test", "test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();

            var error = result.Error!;
            error.ErrorType.Should().Be(HttpErrorType.InternalServerError);
            error.Code.Should().Be("INTERNAL_SERVER_ERROR");
        }

    }
}
