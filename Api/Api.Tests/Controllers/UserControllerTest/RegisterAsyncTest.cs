using Api.Controllers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Responses;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Controllers.UserControllerTest
{
    public class RegisterAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenManager _tokenManager;
        private readonly ResponseHttpFactory _responseFactory;

        public RegisterAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _signInManager = A.Fake<SignInManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _tokenManager = A.Fake<ITokenManager>();
            _responseFactory = A.Fake<ResponseHttpFactory>();
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnOk()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var registerDto = new RegisterDto() { UserName = "test", Password = "test" };
            var identityResult = IdentityResult.Success;

            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                .Returns(Task.FromResult(identityResult));


            // Act
            var result = await userController.RegisterAsync(registerDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as SuccessResponseWithResultDataDto<Dictionary<string, RegisteredUserDto>>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("User has been successfully created.");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnConflict_WhenUserUserNameAlreadyExists()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var registerDto = new RegisterDto() { UserName = "existingUser", Password = "Password" };
            var identityError = new IdentityError { Code = "DuplicateUserName", Description = "Username already exists." };
            var identityResult = IdentityResult.Failed(identityError);

            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                           .Returns(Task.FromResult(identityResult));

            // Act
            var result = await userController.RegisterAsync(registerDto) as ConflictObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(409);

            var response = result.Value as Error409ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(409);
            response.Title.Should().Contain("User with this username already exists.");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnBadRequest_WhenUserManagerReturnError()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var registerDto = new RegisterDto() { UserName = "existingUser", Password = "Password" };
            var identityError = new IdentityError { Code = "", Description = "" };
            var identityResult = IdentityResult.Failed(identityError);

            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                           .Returns(Task.FromResult(identityResult));

            // Act
            var result = await userController.RegisterAsync(registerDto) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);

            var response = result.Value as Error400ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Invalid register attempt.");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var registerDto = new RegisterDto() { UserName = "testUser", Password = "Password" };

            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                           .Throws(new InvalidOperationException("Simulated exception")).Once();

            // Act
            var result = await userController.RegisterAsync(registerDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);

            var response = result.Value as Error500ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(500);
            response.Title.Should().Contain("An internal server error occurred.");
        }
    }
}
