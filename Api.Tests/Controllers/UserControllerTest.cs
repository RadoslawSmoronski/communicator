using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Api.Models;
using Api.Controllers;
using Api.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Tests.Controllers
{
    public class UserControllerTest
    {
        private readonly UserManager<UserAccount> _userManager;

        public UserControllerTest()
        {
            // Initialize UserManager with necessary parameters
            _userManager = A.Fake<UserManager<UserAccount>>();
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnTrue_WhenCalledWithValidParameters()
        {
            // Arrange
            var userController = new UserController(_userManager);
            var registerDto = new RegisterDto() { UserName = "login", Password = "password" };

            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._))
                .Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await userController.RegisterAsync(registerDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as RegisterResponseDto;
            ((string)response!.Message).Should().Be("The user has been successfully created.");
            ((RegisterDto)response.User).Should().BeEquivalentTo(registerDto);
        }
    }
}
