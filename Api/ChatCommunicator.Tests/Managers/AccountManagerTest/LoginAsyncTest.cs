using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public class LoginAsyncTest : AccountManagerTest
    {
        private readonly LoginDto _sampleUser1LoginDto;

        public LoginAsyncTest()
        {
            _sampleUser1LoginDto = new LoginDto()
            {
                UserName = _sampleUser1.UserName!,
                Password = "test"
            };
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByNameAsync(_sampleUser1.UserName!))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _signInManager.PasswordSignInAsync(_sampleUser1, "test", false, false))
                    .Returns(Task.FromResult(SignInResult.Success));

            var fakeRefreshToken = Guid.NewGuid();

            A.CallTo(() => _tokenService.CreateRefreshTokenAsync(_sampleUser1.Id))
                .Returns(Task.FromResult(ResultT<Guid>.Success(fakeRefreshToken)));

            var fakeAccessToken = "test";

            A.CallTo(() => _tokenService.CreateAccessTokenAsync(_sampleUser1))
                .Returns(Task.FromResult(ResultT<string>.Success(fakeAccessToken)));

            var accessToken = await _tokenService.CreateAccessTokenAsync(_sampleUser1);

            var expectedResult = new LoggedUserDto()
            {
                UserName = _sampleUser1.UserName!,
                Id = _sampleUser1.Id,
                AccessToken = fakeAccessToken,
                RefreshToken = fakeRefreshToken
            };

            //Act
            var result = await _accountManager.LoginAsync(_sampleUser1LoginDto) as ResultT<LoggedUserDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUnauthorized_WhenDataIsNotAutharized()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByNameAsync(_sampleUser1.UserName!))
                .Returns(Task.FromResult<UserAccount?>(null));

            //Act
            var result = await _accountManager.LoginAsync(_sampleUser1LoginDto) as ResultT<LoggedUserDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unauthorized);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUnknown_WhenThereIsException()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByNameAsync(_sampleUser1.UserName!))
                .Throws(new Exception());

            //Act
            var result = await _accountManager.LoginAsync(_sampleUser1LoginDto) as ResultT<LoggedUserDto>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}
