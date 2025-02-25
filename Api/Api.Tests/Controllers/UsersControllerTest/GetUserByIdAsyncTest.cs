using Api.Controllers;
using Api.Models.Dtos.Responses;
using Api.Models;
using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Api.Models.Dtos.Controllers.UsersController;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Controllers.UsersControllerTest
{
    public class GetUserByIdAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly UsersController _usersController;
        private readonly UserAccount _sampleUserAccount;

        public GetUserByIdAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _responseHttpFactory = A.Fake<ResponseHttpFactory>();
            _httpContextAccessor = A.Fake<IHttpContextAccessor>();

            _usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);
            _sampleUserAccount = new UserAccount { UserName = "TestLogin123", Id = new Guid().ToString() };
        }

        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnOk()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .Returns(Task.FromResult<UserAccount?>(_sampleUserAccount));

            // Act
            var result = await _usersController.GetUserByIdAsync(_sampleUserAccount.Id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().NotBeNull();

            var response = result.Value as SuccessResponseWithResultDataDto<UsersDto>;
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("User/s has been found.");
        }

        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnBadRequestError_WhenIdIsNullOrEmpty()
        {
            // Act
            var result = await _usersController.GetUserByIdAsync("") as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error400ResponseDto;
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Id cannot be empty.");
        }

        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnBadRequestError_WhenIdHasWrongFormat()
        {
            // Act
            var result = await _usersController.GetUserByIdAsync("test") as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error400ResponseDto;
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Not valid format.");
        }

        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnNotFoundError_WhenUserDoesNotExists()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .Returns(Task.FromResult<UserAccount?>(null));

            // Act
            var result = await _usersController.GetUserByIdAsync(_sampleUserAccount.Id) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error404ResponseDto;
            response!.Status.Should().Be(404);
            response.Title.Should().Contain("User does not exist.");
        }

        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnInternalServerError()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUserAccount.Id))
                           .ThrowsAsync(new Exception());

            // Act
            var result = await _usersController.GetUserByIdAsync(_sampleUserAccount.Id) as ObjectResult;

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
