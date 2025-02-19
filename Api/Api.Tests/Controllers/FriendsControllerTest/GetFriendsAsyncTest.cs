using Api.Controllers;
using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Dtos.Responses;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Utilities.Result;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Controllers.FriendsControllerTest
{
    public class GetFriendsAsyncTest
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;
        private readonly string _validUserId = "062249c3-a5e9-4970-a03d-41a6dd3dc6ed";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetFriendsAsyncTest()
        {
            _friendsManager = A.Fake<IFriendsManager>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _responseHttpFactory = A.Fake<ResponseHttpFactory>();
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnOk()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            var expectedList = new List<FriendDto>()
            {
                new FriendDto() { Id = "testId", UserName = "test"},
                new FriendDto() { Id = "testId2", UserName = "test3"},
                new FriendDto() { Id = "testId2", UserName = "test3"}
            };

            var fakeResult = ResultT<List<FriendDto>>.Success(expectedList);

            A.CallTo(() => _friendsManager.GetFriendsAsync(_validUserId))
                .Returns(Task.FromResult(fakeResult));

            // Act
            var result = await _friendsController.GetFriendsAsync(_validUserId) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            var response = result.Value as SuccessResponseWithResultDataDto<List<FriendDto>>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("Friends found.");
            response.ResultData.Should().BeEquivalentTo(expectedList);
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnBadRequest_WhenUserIdIsEmpty()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            // Act
            var result = await _friendsController.GetFriendsAsync("") as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            var response = result.Value as IResponse;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("UserId is required.");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnBadRequest_WhenUserIdIsNotValid()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            // Act
            var result = await _friendsController.GetFriendsAsync("test") as BadRequestObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            var response = result.Value as IResponse;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("UserId not valid format.");
        }

        [Fact]
        public async Task GetFriendsAsync_ShouldReturnError_WhenFriendsManagerReturnError()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            var fakeResult = ResultT<List<FriendDto>>.Failure(Error.BadRequest("test", "test"));

            A.CallTo(() => _friendsManager.GetFriendsAsync(_validUserId))
                .Returns(Task.FromResult(fakeResult));

            // Act
            var result = await _friendsController.GetFriendsAsync(_validUserId) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
            var response = result.Value as IResponse;
            response.Should().NotBeNull();
            response!.Status.Should().Be(400);
            response.Title.Should().Contain("test");
        }
    }
}
