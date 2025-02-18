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
    public class DecelineInviteAsyncTest
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;
        private readonly string _validUserId = "062249c3-a5e9-4970-a03d-41a6dd3dc6ed";
        private readonly string _validUserId2 = "262245c3-a5e8-4970-a03d-41a6dd3dc6ed";

        public DecelineInviteAsyncTest()
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
        public async Task DecelineInviteAsync_ShouldReturnOk()
        {
            // Arrange
            var decelineInviteDto = new DecelineInviteDto() { SenderId = _validUserId, RecipientId = _validUserId2};
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory);

            A.CallTo(() => _friendsManager.DecelineInviteAsync(_validUserId, _validUserId2))
                .Returns(Task.FromResult(Result.Success()));

            // Act
            var result = await _friendsController.DecelineInviteAsync(decelineInviteDto) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            var response = result.Value as IResponse;
            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Title.Should().Contain("Invitation decelined.");
        }


        [Fact]
        public async Task DecelineInviteAsync_ShouldReturnError_WhenFriendsManagerReturnError() //todo
        {
            // Arrange
            var decelineInviteDto = new DecelineInviteDto() { SenderId = _validUserId, RecipientId = _validUserId2 };
            var _friendsController = new FriendsController(_friendsManager, _mapper, _responseHttpFactory);

            A.CallTo(() => _friendsManager.DecelineInviteAsync(_validUserId, _validUserId2))
                .Returns(Task.FromResult(Result.Failure(Error.Conflict("test", "test2"))));

            // Act
            var result = await _friendsController.DecelineInviteAsync(decelineInviteDto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(409);
            var response = result.Value as IResponse;
            response.Should().NotBeNull();
            response!.Status.Should().Be(409);
            response.Title.Should().Contain("test2");
        }
    }
}
