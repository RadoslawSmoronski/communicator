﻿using FakeItEasy;
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
            response.Succeeded.Should().BeTrue();
            response.Message.Should().Be("The user has been successfully created.");
            response.User.Should().BeEquivalentTo(registerDto);
        }

        [Theory]
        [InlineData("l", "p")]
        [InlineData("lo", "pa")]
        public async Task RegisterAsync_ShouldReturnFalse_WhenCalledWithNotValidParameters(string login, string password)
        {
            // Arrange
            var userController = new UserController(_userManager);
            var registerDto = new RegisterDto() { UserName = login, Password = password };
            var identityResultFailure = IdentityResult.Failed(new IdentityError { Description = "User creation failed." });
            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._))
                .Returns(identityResultFailure);

            // Act
            var result = await userController.RegisterAsync(registerDto) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);

            var response = result.Value as RegisterResponseDto;
            response.Succeeded.Should().BeFalse();
            response.Message.Should().Be("Validation failed");
            response.User.Should().BeEquivalentTo(registerDto);
        }
    }
}
