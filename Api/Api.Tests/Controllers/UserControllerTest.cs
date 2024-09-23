using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Api.Models;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Api.Models.Dtos.Controllers.UserController;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Api.Service;
using System.ComponentModel.DataAnnotations;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;

namespace Api.Tests.Controllers
{
    public class UserControllerTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserControllerTest()
        {
            // Initialize UserManager with necessary parameters
            _userManager = A.Fake<UserManager<UserAccount>>();
            _signInManager = A.Fake<SignInManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _tokenService = A.Fake<ITokenService>();
        }

        //RegisterAsync

        [Fact]
        public async Task RegisterAsync_ShouldReturnConflict_WhenCalledUsernameIsAlreadyExists()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
            var registerDto = new RegisterDto() { UserName = "test", Password = "test" };
            var identityResultFailure = IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "User with this username already exists." });

            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                .Returns(Task.FromResult(identityResultFailure));


            // Act
            var result = await userController.RegisterAsync(registerDto) as ConflictObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(409);

            var response = result.Value as RegisterFailedResponseDto;
            response.Should().NotBeNull();
            response!.Succeeded.Should().BeFalse();
            response.Errors.Should().Contain("User with this username already exists.");
        }

        [Theory]
        [InlineData("123", "123456")]
        public async Task RegisterAsync_ShouldReturnOk_WhenCalledWithValidParameters(string testLogin, string testPassword)
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
            var registerDto = new RegisterDto() { UserName = testLogin, Password = testPassword };

            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                .Returns(IdentityResult.Success);

            A.CallTo(() => _tokenService.CreateRefreshToken())
                            .Returns("fakeRefreshToken");

            // Act
            var result = await userController.RegisterAsync(registerDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as RegisterOkResponseDto;
            response.Should().NotBeNull();
            response!.Succeeded.Should().BeTrue();
            response.Message.Should().Be("The user has been successfully created.");
        }

        //LoginAsync

        [Fact]
        public async Task LoginAsync_ShouldReturnUnauthorized_WhenCalledUsernameDoesntExist()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
            var loginDto = new LoginDto() { UserName = "test", Password = "test" };

            A.CallTo(() => _userManager.FindByNameAsync(loginDto.UserName))
                .Returns(Task.FromResult<UserAccount?>(null));

            A.CallTo(() => _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false))
                    .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.NotAllowed));

            // Act
            var result = await userController.LoginAsync(loginDto) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);

            var response = result.Value as LoginFailedResponseDto;
            response.Should().NotBeNull();
            response!.Succeeded.Should().BeFalse();
            response.Message.Should().Be("User does not exist.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturn500_WhenSignInThrowsException()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
            var loginDto = new LoginDto() { UserName = "test", Password = "test" };

            A.CallTo(() => _userManager.FindByNameAsync(loginDto.UserName))
                .Throws(new Exception("code 500 test"));

            A.CallTo(() => _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false))
                .Throws(new Exception("code 500 test"));

            // Act
            var result = await userController.LoginAsync(loginDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);

            var response = result.Value as LoginFailedResponseDto;
            response.Should().NotBeNull();
            response!.Succeeded.Should().BeFalse();
            response.Message.Should().Be("An internal server error occurred.");
        }

        [Theory]
        [InlineData("123", "123456")]
        public async Task LoginAsync_ShouldReturnOk_WhenCalledWithValidParameters(string testLogin, string testPassword)
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
            var loginDto = new LoginDto() { UserName = "test", Password = "test" };
            var user = new UserAccount { UserName = "test" };

            A.CallTo(() => _userManager.FindByNameAsync(loginDto.UserName))
                .Returns(Task.FromResult<UserAccount?>(user));

            A.CallTo(() => _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));


            //act
            var result = await userController.LoginAsync(loginDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as LoginResponseDto;
            response.Should().NotBeNull();
            response!.Succeeded.Should().BeTrue();
            response.Message.Should().Be("The user has been successfully logged in.");
        }

        //    [Fact]
        //    public async Task RegisterAsync_ShouldReturnBadRequest_WhenCalledWithNotValidParameters()
        //    {
        //        // Arrange
        //        var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
        //        var registerDto = new RegisterDto() { UserName = "test", Password = "password1" };
        //        var identityResultFailure = IdentityResult.Failed(new IdentityError { Description = "User creation failed." });
        //        A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._))
        //            .Returns(identityResultFailure);

        //        // Act
        //        var result = await userController.RegisterAsync(registerDto) as BadRequestObjectResult;

        //        // Assert
        //        result.Should().NotBeNull();
        //        result!.StatusCode.Should().Be(400);

        //        var response = result.Value as RegisterResponseDto;
        //        response.Should().NotBeNull();
        //        response!.Succeeded.Should().BeFalse();
        //    }

        //    //RegisterAsync
        //    //LoginAsync

        //    [Fact]
        //    public async Task LoginAsync_ShouldReturnOk_WhenCalledWithValidParameters()
        //    {
        //        // Arrange
        //        var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
        //        var loginDto = new LoginDto() { UserName = "login", Password = "password" };

        //        A.CallTo(() => _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false))
        //            .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

        //        // Act
        //        var result = await userController.LoginAsync(loginDto) as OkObjectResult;

        //        // Assert
        //        result.Should().NotBeNull();
        //        result!.StatusCode.Should().Be(200);

        //        var response = result.Value as LoginResponseDto;
        //        response.Should().NotBeNull();
        //        response!.Succeeded.Should().BeTrue();
        //        response.Message.Should().Be("The user has been successfully logged in.");
        //    }

        //    [Fact]
        //    public async Task LoginAsync_ShouldReturnBadRequest_WhenCalledWithNotValidParameters()
        //    {
        //        //Arrange
        //        var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
        //        var loginDto = new LoginDto() { UserName = "login", Password = "password" };

        //        A.CallTo(() => _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false))
        //            .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

        //        //Act
        //        var result = await userController.LoginAsync(loginDto) as BadRequestObjectResult;

        //        //Assert
        //        result.Should().NotBeNull();
        //        result!.StatusCode.Should().Be(400);

        //        var response = result.Value as LoginResponseDto;
        //        response.Should().NotBeNull();
        //        response!.Succeeded.Should().BeFalse();
        //    }

        //    //LoginAsync
        //    //DeleteAsyncByUserName

        //    [Fact]
        //    public async Task DeleteAsyncByUserName_ShouldReturnOk_WhenCalledWithValidParameters()
        //    {
        //        //Arrange
        //        var login = "login";
        //        var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
        //        var hasher = new PasswordHasher<IdentityUser>();
        //        var user = new UserAccount()
        //        {
        //            UserName = login
        //        };

        //        A.CallTo(() => _userManager.FindByNameAsync(login))!
        //            .Returns(Task.FromResult<UserAccount>(user));

        //        A.CallTo(() => _userManager.DeleteAsync(user))
        //            .Returns(Task.FromResult(IdentityResult.Success));

        //        //Act
        //        var result = await userController.DeleteUserByUserNameAsync(login) as OkObjectResult;

        //        //Assert
        //        result.Should().NotBeNull();
        //        result!.StatusCode.Should().Be(200);

        //        var response = result.Value as DeleteResponseDto;
        //        response.Should().NotBeNull();
        //        response!.Succeeded.Should().BeTrue();
        //        response.Message.Should().Be("Deleted user.");
        //    }

        //    [Fact]
        //    public async Task DeleteAsyncByUserName_ShouldReturnBadRequest_WhenCalledWithNotValidParameters()
        //    {
        //        //Arrange
        //        var login = "login";
        //        var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
        //        var user = new UserAccount()
        //        {
        //            UserName = login
        //        };

        //        A.CallTo(() => _userManager.FindByNameAsync(login))!
        //            .Returns(Task.FromResult<UserAccount>(user));

        //        //Act
        //        var result = await userController.DeleteUserByUserNameAsync(login) as BadRequestObjectResult;

        //        //Assert
        //        result.Should().NotBeNull();
        //        result!.StatusCode.Should().Be(400);

        //        var response = result.Value as DeleteResponseDto;
        //        response.Should().NotBeNull();
        //        response!.Succeeded.Should().BeFalse();
        //        response.Message.Should().BeOneOf("Not found user.");
        //    }

        //    //DeleteAsyncByUserName
        //    //DeleteAsyncById

        //    [Fact]
        //    public async Task DeleteAsyncById_ShouldReturnOk_WhenCalledWithValidParameters()
        //    {
        //        //Arrange
        //        var id = 1;
        //        var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
        //        var hasher = new PasswordHasher<IdentityUser>();
        //        var user = new UserAccount()
        //        {
        //            UserName = "login"
        //        };

        //        A.CallTo(() => _userManager.FindByIdAsync(id.ToString()))!
        //            .Returns(Task.FromResult(user));

        //        A.CallTo(() => _userManager.DeleteAsync(user))
        //            .Returns(Task.FromResult(IdentityResult.Success));

        //        //Act
        //        var result = await userController.DeleteUserByIdAsync(id) as OkObjectResult;

        //        //Assert
        //        result.Should().NotBeNull();
        //        result!.StatusCode.Should().Be(200);

        //        var response = result.Value as DeleteResponseDto;
        //        response.Should().NotBeNull();
        //        response!.Succeeded.Should().BeTrue();
        //        response.Message.Should().Be("Deleted user.");
        //    }

        //    [Fact]
        //    public async Task DeleteAsyncById_ShouldReturnBadRequest_WhenCalledWithNotValidParameters()
        //    {
        //        //Arrange
        //        var id = 1;
        //        var userController = new UserController(_userManager, _signInManager, _mapper, _tokenService);
        //        var user = new UserAccount()
        //        {
        //            UserName = "login"
        //        };

        //        A.CallTo(() => _userManager.FindByIdAsync(id.ToString()))!
        //            .Returns(Task.FromResult<UserAccount>(user));

        //        //Act
        //        var result = await userController.DeleteUserByIdAsync(id) as BadRequestObjectResult;

        //        //Assert
        //        result.Should().NotBeNull();
        //        result!.StatusCode.Should().Be(400);

        //        var response = result.Value as DeleteResponseDto;
        //        response.Should().NotBeNull();
        //        response!.Succeeded.Should().BeFalse();
        //        response.Message.Should().BeOneOf("Not found user.");
        //    }

        //    //DeleteAsyncById
    }
}
