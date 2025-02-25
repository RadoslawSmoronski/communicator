using Api.Controllers;
using Api.Models.Dtos.Responses;
using Api.Models;
using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Api.Models.Dtos.Controllers.UsersController;
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

        private readonly UsersController _usersController;
        private readonly List<UserAccount> _sampleUsersList;

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

            _usersController = new UsersController(_userManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            _sampleUsersList = new List<UserAccount>
            {
                new UserAccount { Id = Guid.NewGuid().ToString(), UserName = "John1", Email = "user1@example.com" },
                new UserAccount { Id = Guid.NewGuid().ToString(), UserName = "John2", Email = "user2@example.com" },
                new UserAccount { Id = Guid.NewGuid().ToString(), UserName = "AdmJoh3", Email = "user3@example.com" },
                new UserAccount { Id = Guid.NewGuid().ToString(), UserName = "Adm", Email = "user5@example.com" }
            };
        }


        [Fact]
        public async Task GetUserByTextAsyncTest_ShouldReturnOk()
        {
            // Arrange
            var expectedDtoUsers = _mapper.Map<List<UsersDto>>(_sampleUsersList.Where(x => x.UserName != null && x.UserName.Contains("Joh")));

            var users = _sampleUsersList.AsQueryable().BuildMock();

            A.CallTo(() => _userManager.Users).Returns(users);


            // Act
            var result = await _usersController.GetUsersByTextAsync("Joh") as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().NotBeNull();

            var response = result.Value as SuccessResponseWithResultDataDto<List<UsersDto>>;
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("User/s has been found.");
            response.ResultData.Should().BeEquivalentTo(expectedDtoUsers);
        }

        [Fact]
        public async Task GetUserByTextAsyncTest_ShouldReturnOkWithoutCurrentUser_WhenExcludeCurrentUserIsTrue()
        {
            // Arrange
            var textToSearch = "Joh";

            var expectedDtoUsers = _mapper.Map<List<UsersDto>>
                (_sampleUsersList.Where(x => x.UserName != null && x.UserName.Contains(textToSearch) && x.Id != _sampleUsersList[0].Id));

            var users = _sampleUsersList.AsQueryable().BuildMock();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _sampleUsersList[0].Id)
            };

            var identity = new ClaimsIdentity(claims, "mock");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            A.CallTo(() => _userManager.Users).Returns(users);
            A.CallTo(() => _httpContextAccessor.HttpContext)
                .Returns(new DefaultHttpContext { User = claimsPrincipal });


            // Act
            var result = await _usersController.GetUsersByTextAsync(textToSearch, true) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            result.Value.Should().NotBeNull();

            var response = result.Value as SuccessResponseWithResultDataDto<List<UsersDto>>;
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("User/s has been found.");
            response.ResultData.Should().BeEquivalentTo(expectedDtoUsers);
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnBadRequestError_WhenTextIsNullOrEmpty()
        {
            // Act
            var result = await _usersController.GetUsersByTextAsync("") as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error400ResponseDto;
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Input value is empty.");
        }

        [Theory]
        [InlineData("Jo")]
        [InlineData("FSDFSDF$WFSfsdfewrsfsdfefsfesfsfes")]
        public async Task GetUsersByTextAsyncTest_ShouldReturnBadRequestError_WhenTextLengthIsLessThan3OrMoreThan25(string text)
        {
            // Act
            var result = await _usersController.GetUsersByTextAsync(text) as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error400ResponseDto;
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("Username must be between 3 and 25 characters long.");
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnNotFoundError_WhenUserNameDoesNotExists()
        {
            // Arrange
            var users = _sampleUsersList.AsQueryable().BuildMock();

            A.CallTo(() => _userManager.Users).Returns(users);


            // Act
            var result = await _usersController.GetUsersByTextAsync("JohnAdams") as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
            result.Value.Should().NotBeNull();

            var response = result.Value as Error404ResponseDto;
            response!.Status.Should().Be(404);
            response.Title.Should().Contain("User does not exist.");
        }

        [Fact]
        public async Task GetUsersByTextAsyncTest_ShouldReturnInternalServerError()
        {
            // Act
            var result = await _usersController.GetUsersByTextAsync("test") as ObjectResult;

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