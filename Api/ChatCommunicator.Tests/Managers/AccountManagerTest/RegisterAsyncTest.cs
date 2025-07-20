using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public class RegisterAsyncTest : AccountManagerTest
    {
        [Fact]
        public async Task RegisterAsync_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                .Returns(Task.FromResult(IdentityResult.Success));

            var expectedResult = _mapper.Map<SimpleUserDto>(_sampleUser1);

            //Act
            var result = await _accountManager.RegisterAsync(_sampleUser1RegisterDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnConflict_WhenDataIsAlreadyExist()
        {
            // Arrange
            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                .Returns(Task.FromResult(
                    IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicateUserName",
                        Description = "User name already exists."
                    })
                ));

            var expectedResult = _mapper.Map<SimpleUserDto>(_sampleUser1);

            //Act
            var result = await _accountManager.RegisterAsync(_sampleUser1RegisterDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Conflict);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnUnknown_WhenThereIsException()
        {
            // Arrange
            A.CallTo(() => _userManager.CreateAsync(A<UserAccount>._, A<string>._))
                .Throws(new Exception());

            var expectedResult = _mapper.Map<SimpleUserDto>(_sampleUser1);

            //Act
            var result = await _accountManager.RegisterAsync(_sampleUser1RegisterDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}
