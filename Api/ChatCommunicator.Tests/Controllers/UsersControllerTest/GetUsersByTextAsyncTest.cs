using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using MockQueryable;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ChatCommunicator.Models.Dtos;

namespace ChatCommunicator.Tests.Controllers.UsersControllerTest
{
    public class GetUsersByTextAsyncTest : UsersControllerTests
    {
        [Fact]
        public async Task GetUserByTextAsyncTest_ShouldReturnOk()
        {
            // Arrange
            var expectedDtoUsers = _mapper.Map<List<SimpleUserDto>>(_sampleUsersList.Where(x => x.UserName != null && x.UserName.Contains("Joh")));

            var users = _sampleUsersList.AsQueryable().BuildMock();

            A.CallTo(() => _userManager.Users).Returns(users);


            // Act
            var result = await _usersController.GetUsersByTextAsync("Joh") as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().NotBeNull();

            var response = result.Value as List<SimpleUserDto>;
        }

        [Fact]
        public async Task GetUserByTextAsyncTest_ShouldReturnOkWithoutCurrentUser_WhenExcludeCurrentUserIsTrue()
        {
            // Arrange
            var textToSearch = "Joh";

            var expectedDtoUsers = _mapper.Map<List<SimpleUserDto>>
                (_sampleUsersList.Where(x => x.UserName != null && x.UserName.Contains(textToSearch) && x.Id != _sampleUsersList[0].Id));

            var users = _sampleUsersList.AsQueryable().BuildMock();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _sampleUsersList[0].Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "mock");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            A.CallTo(() => _userManager.Users).Returns(users);
            A.CallTo(() => _httpContextAccessor.HttpContext)
                .Returns(new DefaultHttpContext { User = claimsPrincipal });


            // Act
            var result = await _usersController.GetUsersByTextAsync(textToSearch, true) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().NotBeNull();

            var response = result.Value as List<SimpleUserDto>;
            response.Should().BeEquivalentTo(expectedDtoUsers);
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnBadRequestError_WhenTextIsNullOrEmpty()
        {
            // Act
            var result = await _usersController.GetUsersByTextAsync("") as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
        }

        [Theory]
        [InlineData("Jo")]
        [InlineData("FSDFSDF$WFSfsdfewrsfsdfefsfesfsfes")]
        public async Task GetUsersByTextAsyncTest_ShouldReturnBadRequestError_WhenTextLengthIsLessThan3OrMoreThan25(string text)
        {
            // Act
            var result = await _usersController.GetUsersByTextAsync(text) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnInternalServerError()
        {
            // Act
            var result = await _usersController.GetUsersByTextAsync("test") as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
        }

    }
}