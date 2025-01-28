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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Controllers.FriendsControllerTest
{
    public class GetInvitationsAsyncTest
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;

        public GetInvitationsAsyncTest()
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
        public async Task GetInvitationsAsync_ShouldReturnOk()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory);

            var sendInviteDto = new SendInviteDto() { SenderId = "1", RecipientId = "2" };

            var expectedList = new List<GetInvitationsUserDto>()
            {
                new GetInvitationsUserDto() { Id = "1", UserName = "test"}
            };

            var fakeResult = ResultT<List<GetInvitationsUserDto>>.Success(expectedList);

            A.CallTo(() => _friendsManager.GetInvitationsAsync("test"))
                .Returns(Task.FromResult(fakeResult));

            // Act
            var result = await _friendsController.GetInvitationsAsync("test") as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            var response = result.Value as SuccessResponseWithResultDataDto<List<GetInvitationsUserDto>>;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("Invitations found.");
            response.ResultData.Should().BeEquivalentTo(expectedList);
        }

        [Fact]
        public async Task GetInvitationsAsync_ShouldReturnError_WhenFriendsManagerReturnError()
        {
            // Arrange
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory);

            var sendInviteDto = new SendInviteDto() { SenderId = "1", RecipientId = "2" };

            var fakeResult = ResultT<List<GetInvitationsUserDto>>.Failure(Error.BadRequest("test", "test"));

            A.CallTo(() => _friendsManager.GetInvitationsAsync("test"))
                .Returns(Task.FromResult(fakeResult));

            // Act
            var result = await _friendsController.GetInvitationsAsync("test") as ObjectResult;

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
