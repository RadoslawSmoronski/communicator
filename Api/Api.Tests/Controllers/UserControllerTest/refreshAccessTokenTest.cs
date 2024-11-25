using Api.Managers.Interfaces;
using Api.Models.Dtos.Responses;
using Api.Models;
using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Controllers;
using Api.Models.Dtos.Controllers.UserController;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Api.Utilities.Result;
using Api.Models.Dtos.Responses.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Api.Data.Repository;
using Api.Service;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Tests.Controllers.UserControllerTest
{
    public class refreshAccessTokenTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenManager _tokenManager;
        private readonly ResponseHttpFactory _responseFactory;

        public refreshAccessTokenTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _signInManager = A.Fake<SignInManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _tokenManager = A.Fake<ITokenManager>();
            _responseFactory = A.Fake<ResponseHttpFactory>();

        }


        [Fact]
        public async Task refreshAccessTokenAsync_ShouldReturnOk()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var tokensResult = ResultT<string>.Success("test");
            var refreshTokenDto = new RefreshTokenDto() { RefreshToken = "cE1B4f9D-2d91-1c57-eFB4-36D4118BabAC" };

            A.CallTo(() => _tokenManager.RefreshAccessTokenAsync(A<string>._))
                .Returns(tokensResult);


            // Act
            var result = await userController.refreshAccessTokenAsync(refreshTokenDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as SuccessResponseWithResultDataDto<string>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("The access token have been successfully refreshed.");
        }

        [Fact]
        public async Task refreshAccessTokenAsync_ShouldReturnNotFound()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var error = Utilities.Result.Error.NotFound("REFRESHTOKEN_NOT_FOUND", "Refresh token not found.");
            var tokensResult = ResultT<string>.Failure(error);

            var refreshTokenDto = new RefreshTokenDto() { RefreshToken = "cE1B4f9D-2d91-1c57-eFB4-36D4118BabAC" };

            A.CallTo(() => _tokenManager.RefreshAccessTokenAsync(refreshTokenDto.RefreshToken))
                .Returns(tokensResult);

            A.CallTo(() => _mapper.Map<ResponseHttpType>(error.ErrorType))
                .Returns(ResponseHttpType.NotFound).Once();


            // Act
            var result = await userController.refreshAccessTokenAsync(refreshTokenDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);

            var response = result.Value as Error404ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(404);
            response.Title.Should().Contain("Refresh token not found.");
        }

        [Fact]
        public async Task refreshAccessTokenAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager, _responseFactory);
            var error = Utilities.Result.Error.InternalServerError("USER_ERROR", "User form refresh token record doesn't exist.");
            var tokensResult = ResultT<string>.Failure(error);
            var refreshTokenDto = new RefreshTokenDto() { RefreshToken = "cE1B4f9D-2d91-1c57-eFB4-36D4118BabAC" };

            A.CallTo(() => _tokenManager.RefreshAccessTokenAsync(refreshTokenDto.RefreshToken))
                           .Returns(tokensResult);

            A.CallTo(() => _mapper.Map<ResponseHttpType>(error.ErrorType))
                           .Returns(ResponseHttpType.InternalServerError).Once(); ;

            // Act
            var result = await userController.refreshAccessTokenAsync(refreshTokenDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);

            var response = result.Value as Error500ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(500);
            response.Title.Should().Contain("User form refresh token record doesn't exist.");
        }

    }
}
