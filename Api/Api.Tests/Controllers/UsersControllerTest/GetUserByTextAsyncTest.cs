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

namespace Api.Tests.Controllers.UsersControllerTest
{
    public class GetUserByTextAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;

        public GetUserByTextAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _mapper = A.Fake<IMapper>();
            _responseHttpFactory = A.Fake<ResponseHttpFactory>();
        }

        [Fact]
        public async Task GetUserByIdAsyncTest_ShouldReturnOk()
        {
            // Arrange
            var usersController = new UsersController(_userManager, _mapper, _responseHttpFactory);
            //var usersDto = new UsersDto { UserName = "testName", Id = "cE1B4f9D-2d91-1c57-eFB4-36D4118BabAC" };
            //var user = new UserAccount { UserName = usersDto.UserName, Id = usersDto.Id };

            var newUsers = new List<UserAccount>
            {
                new UserAccount { Id = "test", UserName = "John1", Email = "user1@example.com" },
                new UserAccount { Id = "test2", UserName = "John2", Email = "user2@example.com" }
            };

            var users = newUsers.AsQueryable().BuildMock();

            A.CallTo(() => _userManager.Users).Returns(users);


            // Act
            var result = await usersController.getUsersByTextAsync("Jo") as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

    }
}
