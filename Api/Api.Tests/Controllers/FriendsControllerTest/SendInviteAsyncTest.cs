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
    public class SendInviteAsyncTest
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SendInviteAsyncTest()
        {
            _friendsManager = A.Fake<IFriendsManager>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _responseHttpFactory = A.Fake<ResponseHttpFactory>();
            _httpContextAccessor = A.Fake<HttpContextAccessor>();
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnOk()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            var sendInviteDto = new SendInviteDto() { SenderId = "1", RecipientId = "2" };

            A.CallTo(() => _friendsManager.SendInviteAsync("1", "2"))
                .Returns(Task.FromResult(Result.Success()));

            // Act
            var result = await _friendsController.SendInviteAsync(sendInviteDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            var response = result.Value as IResponse;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("Invitation sent.");
        }

        [Fact]
        public async Task SendInviteAsync_ShouldReturnError_WhenFriendsManagerReturnError()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory, _httpContextAccessor);

            var sendInviteDto = new SendInviteDto() { SenderId = "1", RecipientId = "2" };

            A.CallTo(() => _friendsManager.SendInviteAsync("1", "2"))
                .Returns(Task.FromResult(Result.Failure(Error.Conflict("test", "test2"))));

            // Act
            var result = await _friendsController.SendInviteAsync(sendInviteDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(409);
            var response = result.Value as IResponse;
            response.Should().NotBeNull();
            response!.Status.Should().Be(409);
            response.Title.Should().Contain("test");
        }
    }
}
