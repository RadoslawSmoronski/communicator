using Api.Controllers;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using Api.Models.Dtos.Responses;
using Api.Models;
using Api.Utilities.Result;
using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Api.Models.Dtos.Controllers.UsersController;

namespace Api.Tests.Controllers.UsersControllerTest
{
    public class GetUserByIdAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;

        public GetUserByIdAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _responseHttpFactory = A.Fake<ResponseHttpFactory>();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnOk()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory);
            var usersDto = new UsersDto { UserName = "testName", Id = "cE1B4f9D-2d91-1c57-eFB4-36D4118BabAC" };
            var user = new UserAccount { UserName = usersDto.UserName, Id = usersDto.Id };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult<UserAccount?>(user));

            // Act
            var result = await usersController.GetUserByIdAsync(user.Id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as SuccessResponseWithResultDataDto<UsersDto>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("User found.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnBadRequestError_WhenIdIsNullOrEmpty()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory);

            // Act
            var result = await usersController.GetUserByIdAsync("") as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);

            var response = result.Value as Error400ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Id is required.");
        }
    }
}
