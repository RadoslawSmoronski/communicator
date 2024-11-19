using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Controllers.UserController
{
    public class RegisterAsyncTest
    {
        private readonly UserManager<UserAccount> _userManager;

        public RegisterAsyncTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnConflict_WhenCalledUsernameIsAlreadyExists()
        {
            //// Arrange
            //var userController = new UserController(_userManager, _signInManager, _mapper, _tokenManager);
            //var registerDto = new RegisterDto() { UserName = "test", Password = "test" };
            //var identityResultFailure = IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "User with this username already exists." });

            //A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
            //    .Returns(Task.FromResult(identityResultFailure));


            //// Act
            //var result = await userController.RegisterAsync(registerDto) as ConflictObjectResult;

            //// Assert
            //result.Should().NotBeNull();
            //result!.StatusCode.Should().Be(409);

            //var response = result.Value as RegisterFailedResponseDto;
            //response.Should().NotBeNull();
            //response!.Succeeded.Should().BeFalse();
            //response.Errors.Should().Contain("User with this username already exists.");
        }
    }
}
