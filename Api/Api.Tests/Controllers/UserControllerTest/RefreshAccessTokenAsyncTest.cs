//using Api.Managers.Interfaces;
//using Api.Models.Dtos.Responses;
//using Api.Models;
//using AutoMapper;
//using FakeItEasy;
//using Microsoft.AspNetCore.Identity;
//using Api.Controllers;
//using Api.Models.Dtos.Controllers.UserController;
//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc;
//using Api.Utilities.Result;
//using Api.Models.Dtos.Responses.Interfaces;

//namespace Api.Tests.Controllers.UserControllerTest
//{
//    public class RefreshAccessTokenAsyncTest
//    {
//        private readonly UserManager<UserAccount> _userManager;
//        private readonly SignInManager<UserAccount> _signInManager;
//        private readonly IMapper _mapper;
//        private readonly ITokenManager _tokenManager;
//        private readonly ResponseHttpFactory _responseFactory;

//        private readonly UserController _userController;
//        private readonly RefreshTokenDto _samplerefreshTokenDto;

//        public RefreshAccessTokenAsyncTest()
//        {
//            _userManager = A.Fake<UserManager<UserAccount>>();
//            _signInManager = A.Fake<SignInManager<UserAccount>>();
//            _mapper = A.Fake<IMapper>();
//            _tokenManager = A.Fake<ITokenManager>();
//            _responseFactory = A.Fake<ResponseHttpFactory>();

//            _userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
//            _samplerefreshTokenDto = new RefreshTokenDto() { RefreshToken = "cE1B4f9D-2d91-1c57-eFB4-36D4118BabAC" };
//        }


//        [Fact]
//        public async Task RefreshAccessTokenAsync_ShouldReturnOk()
//        {
//            // Arrange
//            var tokensResult = ResultT<string>.Success("test"); 

//            A.CallTo(() => _tokenManager.RefreshAccessTokenAsync(A<string>._))
//                .Returns(tokensResult);


//            // Act
//            var result = await _userController.RefreshAccessTokenAsync(_samplerefreshTokenDto) as OkObjectResult;

//            // Assert
//            result.Should().NotBeNull();
//            result!.StatusCode.Should().Be(200);
//            result.Value.Should().NotBeNull();

//            var response = result.Value as SuccessResponseWithResultDataDto<string>;
//            response!.Status.Should().Be(200);
//            response.Title.Should().Contain("The access token has been successfully refreshed.");
//        }

//        [Fact]
//        public async Task RefreshAccessTokenAsync_ShouldReturnNotFound()
//        {
//            // Arrange
//            var error = Error.NotFound("REFRESHTOKEN_NOT_FOUND", "Refresh token not found.");
//            var tokensResult = ResultT<string>.Failure(error);

//            A.CallTo(() => _tokenManager.RefreshAccessTokenAsync(_samplerefreshTokenDto.RefreshToken))
//                .Returns(tokensResult);

//            A.CallTo(() => _mapper.Map<ResponseHttpType>(error.ErrorType))
//                .Returns(ResponseHttpType.NotFound).Once();


//            // Act
//            var result = await _userController.RefreshAccessTokenAsync(_samplerefreshTokenDto) as ObjectResult;

//            // Assert
//            result.Should().NotBeNull();
//            result!.StatusCode.Should().Be(404);
//            result.Value.Should().NotBeNull();

//            var response = result.Value as Error404ResponseDto;
//            response!.Status.Should().Be(404);
//            response.Title.Should().Contain("Refresh token not found.");
//        }

//        [Fact]
//        public async Task RefreshAccessTokenAsync_ShouldReturnInternalServerError()
//        {
//            // Arrange
//            var error = Error.InternalServerError("USER_ERROR", "User associated with the refresh token record doesn't exist.");
//            var tokensResult = ResultT<string>.Failure(error);

//            A.CallTo(() => _tokenManager.RefreshAccessTokenAsync(_samplerefreshTokenDto.RefreshToken))
//                           .Returns(tokensResult);

//            A.CallTo(() => _mapper.Map<ResponseHttpType>(error.ErrorType))
//                           .Returns(ResponseHttpType.InternalServerError).Once(); ;

//            // Act
//            var result = await _userController.RefreshAccessTokenAsync(_samplerefreshTokenDto) as ObjectResult;

//            // Assert
//            result.Should().NotBeNull();
//            result!.StatusCode.Should().Be(500);
//            result.Value.Should().NotBeNull();

//            var response = result.Value as Error500ResponseDto;
//            response!.Status.Should().Be(500);
//            response.Title.Should().Contain("User associated with the refresh token record doesn't exist.");
//        }


//    }
//}
