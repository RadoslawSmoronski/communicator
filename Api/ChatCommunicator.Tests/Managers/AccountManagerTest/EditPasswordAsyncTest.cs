using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public class ChangePasswordAsyncTest : AccountManagerTest
    {

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                    .Returns(Task.FromResult<UserAccount?>(_sampleUser1));


            A.CallTo(() => _userManager.ChangePasswordAsync(_sampleUser1, "test1", "test2"))
                .Returns(Task.FromResult(IdentityResult.Success));

            //Act
            var result = await _accountManager.ChangePasswordAsync(_sampleUser1.Id, "test1", "test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Theory]
        [InlineData(SAMPLE_STRING_EMPTY_GUID, "test1", "test2")]
        [InlineData(SAMPLE_STRING_GUID, "", "test2")]
        [InlineData(SAMPLE_STRING_GUID, "test1", "")]
        [InlineData(SAMPLE_STRING_GUID, "test1", "test1")]
        public async Task ChangePasswordAsync_ShouldReturnValidationError_WhenDataAreEmptyOrOldAndNewPasswordsAreTheSame(string value1, string value2, string value3)
        {
            // Arrange

            //Act
            var result = await _accountManager.ChangePasswordAsync(Guid.Parse(value1), value2, value3) as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error!.ErrorType.Should().Be(ErrorType.Validation);
        }


        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnUnauthorized_WhenUserWithUserIdDoesntExist()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(null));

            //Act
            var result = await _accountManager.ChangePasswordAsync(_sampleUser1.Id, "test1", "test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unauthorized);
            result.Error!.Code.Should().Be("USER_NOT_FOUND");
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnUnauthorized_WhenOldPasswordIsIncorect()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.ChangePasswordAsync(_sampleUser1, "test1", "test2"))
             .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "The old password is incorrect." })));

            //Act
            var result = await _accountManager.ChangePasswordAsync(_sampleUser1.Id, "test1", "test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unauthorized);
            result.Error!.Code.Should().Be("OLDPASSWORD_IS_INCORRECT");
        }

        [Theory]
        [InlineData("PasswordRequireDigit")]
        [InlineData("PasswordRequireLower")]
        [InlineData("PasswordRequireNonLetterOrDigit")]
        [InlineData("PasswordRequireUpper")]
        [InlineData("PasswordTooShort")]
        public async Task ChangePasswordAsync_ShouldReturnValidation_WhenNewPasswordIsNotValid(string error)
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Returns(Task.FromResult<UserAccount?>(_sampleUser1));

            A.CallTo(() => _userManager.ChangePasswordAsync(_sampleUser1, "test1", "test2"))
             .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Code = error })));

            //Act
            var result = await _accountManager.ChangePasswordAsync(_sampleUser1.Id, "test1", "test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Validation);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnUnknown_WhenThereIsException()
        {
            // Arrange
            A.CallTo(() => _userManager.FindByIdAsync(_sampleUser1.Id.ToString()))
                .Throws(new Exception());

            //Act
            var result = await _accountManager.ChangePasswordAsync(_sampleUser1.Id, "test1", "test2") as Result;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.ErrorType.Should().Be(ErrorType.Unknown);
        }

    }
}