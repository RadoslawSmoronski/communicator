//using Api.Controllers;
//using Api.Managers.Interfaces;
//using Api.Models;
//using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
//using Api.Models.Dtos.Responses;
//using AutoMapper;
//using FakeItEasy;
//using FluentAssertions;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace Api.Tests.Controllers.UserControllerTest
//{
//    public class RegisterAsyncTest
//    {
//        private readonly UserManager<UserAccount> _userManager;
//        private readonly SignInManager<UserAccount> _signInManager;
//        private readonly IMapper _mapper;
//        private readonly ITokenManager _tokenManager;
//        private readonly ResponseHttpFactory _responseFactory;

//        private readonly UserController _userController;
//        private readonly RegisterDto _sampleRegisterDto;

//        public RegisterAsyncTest()
//        {
//            _userManager = A.Fake<UserManager<UserAccount>>();
//            _signInManager = A.Fake<SignInManager<UserAccount>>();
//            _mapper = A.Fake<IMapper>();
//            _tokenManager = A.Fake<ITokenManager>();
//            _responseFactory = A.Fake<ResponseHttpFactory>();

//            _userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
//            _sampleRegisterDto = new RegisterDto() { UserName = "test", Password = "test" };
//        }

//        [Fact]
//        public async Task RegisterAsync_ShouldReturnOk()
//        {
//            // Arrange
//            var identityResult = IdentityResult.Success;

//            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
//                .Returns(Task.FromResult(identityResult));


//            // Act
//            var result = await _userController.RegisterAsync(_sampleRegisterDto) as OkObjectResult;

//            // Assert
//            result.Should().NotBeNull();
//            result!.StatusCode.Should().Be(200);
//            result.Value.Should().NotBeNull();

//            var response = result.Value as SuccessResponseWithResultDataDto<Dictionary<string, RegisteredUserDto>>;
//            response!.Status.Should().Be(200);
//            response.Title.Should().Contain("The user has been successfully created.");
//        }

//        [Fact]
//        public async Task RegisterAsync_ShouldReturnConflict_WhenUserUserNameAlreadyExists()
//        {
//            // Arrange
//            var identityError = new IdentityError { Code = "DuplicateUserName", Description = "Username already exists." };
//            var identityResult = IdentityResult.Failed(identityError);

//            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
//                           .Returns(Task.FromResult(identityResult));

//            // Act
//            var result = await _userController.RegisterAsync(_sampleRegisterDto) as ConflictObjectResult;

//            // Assert
//            result.Should().NotBeNull();
//            result!.StatusCode.Should().Be(409);
//            result.Value.Should().NotBeNull();

//            var response = result.Value as Error409ResponseDto;
//            response!.Status.Should().Be(409);
//            response.Title.Should().Contain("A user with this username already exists.");
//        }

//        [Fact]
//        public async Task RegisterAsync_ShouldReturnBadRequest_WhenUserManagerReturnError()
//        {
//            // Arrange
//            var identityError = new IdentityError { Code = "", Description = "" };
//            var identityResult = IdentityResult.Failed(identityError);

//            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
//                           .Returns(Task.FromResult(identityResult));

//            // Act
//            var result = await _userController.RegisterAsync(_sampleRegisterDto) as BadRequestObjectResult;

//            // Assert
//            result.Should().NotBeNull();
//            result!.StatusCode.Should().Be(400);
//            result.Value.Should().NotBeNull();

//            var response = result.Value as Error400ResponseDto;
//            response!.Status.Should().Be(400);
//            response.Title.Should().Contain("Invalid registration attempt.");
//        }

//        [Fact]
//        public async Task RegisterAsync_ShouldReturnInternalServerError()
//        {
//            // Arrange      
//            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
//                           .Throws(new InvalidOperationException("Simulated exception")).Once();

//            // Act
//            var result = await _userController.RegisterAsync(_sampleRegisterDto) as ObjectResult;

//            // Assert
//            result.Should().NotBeNull();
//            result!.StatusCode.Should().Be(500);
//            result.Value.Should().NotBeNull();

//            var response = result.Value as Error500ResponseDto;
//            response!.Status.Should().Be(500);
//            response.Title.Should().Contain("An internal server error occurred.");
//        }
//    }
//}
