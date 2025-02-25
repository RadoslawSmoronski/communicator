using Api.Managers.Interfaces;
using Api.Models.Dtos.Responses;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using Api.Controllers;
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

        private readonly UserController _userController;
        private readonly LoginDto _sampleLoginDto;
        private readonly UserAccount _sampleUser;

        public LoginAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _signInManager = A.Fake<SignInManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _tokenManager = A.Fake<ITokenManager>();
            _responseFactory = A.Fake<ResponseHttpFactory>();

            _userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            _sampleLoginDto = new LoginDto() { UserName = "existingUser", Password = "test" };
            _sampleUser = new UserAccount { UserName = _sampleLoginDto.UserName, Id = new Guid().ToString() };
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnOk()
        {
            // Arrange
            var identityResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            var tokensResult = ResultT<string>.Success("test");

            A.CallTo(() => _userManager.FindByNameAsync(_sampleLoginDto.UserName))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _signInManager.PasswordSignInAsync(A<UserAccount>._, A<string>._, false, false))
                           .Returns(Task.FromResult(identityResult));

            A.CallTo(() => _tokenManager.CreateRefreshTokenAsync(_sampleUser.Id))
                           .Returns(tokensResult);

            A.CallTo(() => _tokenManager.CreateAccessTokenAsync(_sampleUser))
                           .Returns(tokensResult);

            //act
            var result = await _userController.LoginAsync(_sampleLoginDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().NotBeNull();

            var response = result.Value as SuccessResponseWithResultDataDto<Dictionary<string, LoggedUserDto>>;
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("The user has been successfully logged in.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNotFound_WhenUserDoesntExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByNameAsync(_sampleLoginDto.UserName))
                           .Returns(Task.FromResult<UserAccount?>(null));

            //act
            var result = await _userController.LoginAsync(_sampleLoginDto) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error404ResponseDto;
            response!.Status.Should().Be(404);
            response.Title.Should().Contain("No user with this username exists.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnBadRequest_WhenUserManagerReturnError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByNameAsync(_sampleLoginDto.UserName))
               .Returns(Task.FromResult<UserAccount?>(_sampleUser));

            A.CallTo(() => _signInManager.PasswordSignInAsync(A<UserAccount>._, A<string>._, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            // Act
            var result = await _userController.LoginAsync(_sampleLoginDto) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error400ResponseDto;
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Invalid login attempt.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var identityResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            var tokensResult = ResultT<string>.Success("test");

            A.CallTo(() => _userManager.FindByNameAsync(_sampleLoginDto.UserName))
                           .Throws(new InvalidOperationException("Simulated exception"));

            A.CallTo(() => _signInManager.PasswordSignInAsync(A<UserAccount>._, A<string>._, false, false))
                           .Returns(Task.FromResult(identityResult));

            A.CallTo(() => _tokenManager.CreateRefreshTokenAsync(_sampleUser.Id))
                           .Returns(tokensResult);

            A.CallTo(() => _tokenManager.CreateAccessTokenAsync(_sampleUser))
                           .Returns(tokensResult);

            //act
            var result = await _userController.LoginAsync(_sampleLoginDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error500ResponseDto;
            response!.Status.Should().Be(500);
            response.Title.Should().Contain("An internal server error occurred.");
        }
    }
}
