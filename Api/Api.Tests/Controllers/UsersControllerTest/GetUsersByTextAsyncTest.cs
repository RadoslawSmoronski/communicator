using Api.Controllers;
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
using static System.Net.Mime.MediaTypeNames;
using MockQueryable;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Api.Tests.Controllers.UsersControllerTest
{
    public class GetUsersByTextAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetUsersByTextAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _responseHttpFactory = A.Fake<ResponseHttpFactory>();
            _httpContextAccessor = A.Fake<IHttpContextAccessor>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();
        }


        [Fact]
        public async Task GetUserByTextAsyncTest_ShouldReturnOk()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            var newUsers = new List<UserAccount>
            {
                new UserAccount { Id = "test", UserName = "John1", Email = "user1@example.com" },
                new UserAccount { Id = "test2", UserName = "John2", Email = "user2@example.com" },
                new UserAccount { Id = "test3", UserName = "AdmJoh3", Email = "user3@example.com" },
                new UserAccount { Id = "test4", UserName = "Adm", Email = "user5@example.com" }
            };

            var expectedDtoUsers = _mapper.Map<List<UsersDto>>(newUsers.Where(x => x.UserName.Contains("Joh")));

            var users = newUsers.AsQueryable().BuildMock();

            A.CallTo(() => _userManager.Users).Returns(users);


            // Act
            var result = await usersController.GetUsersByTextAsync("Joh") as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as SuccessResponseWithResultDataDto<List<UsersDto>>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("User/s found.");
            response.ResultData.Should().BeEquivalentTo(expectedDtoUsers);
        }

        [Fact]
        public async Task GetUserByTextAsyncTest_ShouldReturnOkWithoutCurrentUser_WhenExcludeCurrentUserIsTrue()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            var newUsers = new List<UserAccount>
            {
                new UserAccount { Id = "test", UserName = "John1", Email = "user1@example.com" },
                new UserAccount { Id = "test2", UserName = "John2", Email = "user2@example.com" },
                new UserAccount { Id = "test3", UserName = "AdmJoh3", Email = "user3@example.com" },
                new UserAccount { Id = "test4", UserName = "Adm", Email = "user5@example.com" }
            };

            var textToSearch = "Joh";

            var expectedDtoUsers = _mapper.Map<List<UsersDto>>
                (newUsers.Where(x => x.UserName.Contains(textToSearch) && x.Id != "test"));

            var users = newUsers.AsQueryable().BuildMock();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test")
            };

            var identity = new ClaimsIdentity(claims, "mock");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            A.CallTo(() => _userManager.Users).Returns(users);
            A.CallTo(() => _httpContextAccessor.HttpContext.User).Returns(claimsPrincipal);

            // Act
            var result = await usersController.GetUsersByTextAsync(textToSearch, true) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            var response = result.Value as SuccessResponseWithResultDataDto<List<UsersDto>>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("User/s found.");
            response.ResultData.Should().BeEquivalentTo(expectedDtoUsers);
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnBadRequestError_WhenTextIsNullOrEmpty()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            // Act
            var result = await usersController.GetUsersByTextAsync("") as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);

            var response = result.Value as Error400ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Input value is empty.");
        }

        [Theory]
        [InlineData("Jo")]
        [InlineData("FSDFSDF$WFSfsdfewrsfsdfefsfesfsfes")]
        public async Task GetUsersByTextAsyncTest_ShouldReturnBadRequestError_WhenTextLengthIsLessThan3OrMoreThan25(string text)
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            // Act
            var result = await usersController.GetUsersByTextAsync(text) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);

            var response = result.Value as Error400ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Username must be more than 3 characters and less than 25.");
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnNotFoundError_WhenUserNameDoesNotExists()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            var newUsers = new List<UserAccount>
            {
                new UserAccount { Id = "test", UserName = "John1", Email = "user1@example.com" },
                new UserAccount { Id = "test2", UserName = "John2", Email = "user2@example.com" },
                new UserAccount { Id = "test3", UserName = "AdmJoh3", Email = "user3@example.com" },
                new UserAccount { Id = "test4", UserName = "Adm", Email = "user5@example.com" }
            };

            var users = newUsers.AsQueryable().BuildMock();

            A.CallTo(() => _userManager.Users).Returns(users);


            // Act
            var result = await usersController.GetUsersByTextAsync("JohnAdams") as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);

            var response = result.Value as Error404ResponseDto;
            response.Should().NotBeNull();
            response!.Status.Should().Be(404);
            response.Title.Should().Contain("There is no user with this username.");
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnInternalServerError()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            // Act
            var result = await usersController.GetUsersByTextAsync("test") as ObjectResult;

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
