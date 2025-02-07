using Api.Managers.Interfaces;
using Api.Models.Dtos.Responses;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Api.Controllers;
using Api.Models.Dtos.Controllers.UserController;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Api.Utilities.Result;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;

namespace Api.Tests.Controllers.UserControllerTest
{
    public class LoginAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenManager _tokenManager;
        private readonly ResponseHttpFactory _responseFactory;

        public LoginAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _signInManager = A.Fake<SignInManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _tokenManager = A.Fake<ITokenManager>();
            _responseFactory = A.Fake<ResponseHttpFactory>();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnOk()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var loginDto = new LoginDto() { UserName = "existingUser", Password = "test" };
            var user = new UserAccount { UserName = loginDto.UserName };
            var identityResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            var tokensResult = ResultT<string>.Success("test");

            A.CallTo(() => _userManager.FindByNameAsync(loginDto.UserName))
                           .Returns(Task.FromResult<UserAccount?>(user));

            A.CallTo(() => _signInManager.PasswordSignInAsync(A<UserAccount>._, A<string>._, false, false))
                           .Returns(Task.FromResult(identityResult));

            A.CallTo(() => _tokenManager.CreateRefreshTokenAsync(user.Id))
                           .Returns(tokensResult);

            A.CallTo(() => _tokenManager.CreateAccessTokenAsync(user))
                           .Returns(tokensResult);

            //act
            var result = await userController.LoginAsync(loginDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as SuccessResponseWithResultDataDto<Dictionary<string, LoggedUserDto>>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("The user has been successfully logged in.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNotFound_WhenUserDoesntExists()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var loginDto = new LoginDto() { UserName = "existingUser", Password = "test" };

            A.CallTo(() => _userManager.FindByNameAsync(loginDto.UserName))
                           .Returns(Task.FromResult<UserAccount?>(null));

            //act
            var result = await userController.LoginAsync(loginDto) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);

            var response = result.Value as Error404ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(404);
            response.Title.Should().Contain("A user with this username does not exist.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnBadRequest_WhenUserManagerReturnError()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var loginDto = new LoginDto() { UserName = "notExistingUser", Password = "test" };
            var user = new UserAccount { UserName = loginDto.UserName };

            A.CallTo(() => _userManager.FindByNameAsync(loginDto.UserName))
               .Returns(Task.FromResult<UserAccount?>(user));

            A.CallTo(() => _signInManager.PasswordSignInAsync(A<UserAccount>._, A<string>._, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            // Act
            var result = await userController.LoginAsync(loginDto) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);

            var response = result.Value as Error400ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Invalid login attempt.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var loginDto = new LoginDto() { UserName = "existingUser", Password = "test" };
            var user = new UserAccount { UserName = loginDto.UserName };
            var identityResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            var tokensResult = ResultT<string>.Success("test");

            A.CallTo(() => _userManager.FindByNameAsync(loginDto.UserName))
                           .Throws(new InvalidOperationException("Simulated exception"));

            A.CallTo(() => _signInManager.PasswordSignInAsync(A<UserAccount>._, A<string>._, false, false))
                           .Returns(Task.FromResult(identityResult));

            A.CallTo(() => _tokenManager.CreateRefreshTokenAsync(user.Id))
                           .Returns(tokensResult);

            A.CallTo(() => _tokenManager.CreateAccessTokenAsync(user))
                           .Returns(tokensResult);

            //act
            var result = await userController.LoginAsync(loginDto) as ObjectResult;

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
