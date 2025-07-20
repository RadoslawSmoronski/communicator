using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Infrastructure.Models;

namespace ChatCommunicator.Tests.Controllers.UsersControllerTest
{
    public class GetUserByIdAsyncTest : UsersControllerTests
    {
        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _usersController.GetUserByIdAsync(_sampleUserAccount.Id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().NotBeNull();

            var response = result.Value as SimpleUserDto;
        }

        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id.ToString()))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _usersController.GetUserByIdAsync(_sampleUserAccount.Id) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
        }
    }
}
