using Api.Managers.Interfaces;
using Api.Models.Dtos.Responses;
using Api.Models;
using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Controllers;
using Api.Models.Dtos.Controllers.UserController;
using Api.Utilities.Result;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Api.Data.IRepository;
using Api.Data.Repository;
using Api.Service;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Collections;
using Microsoft.AspNetCore.Hosting.Server;

namespace Api.Tests.Managers.TokenManagerTest
{
    public class CreateAccessTokenAsyncTest
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly ITokenManager _tokenManager;

        public CreateAccessTokenAsyncTest()
        {
            _refreshTokenRepository = A.Fake<IRefreshTokenRepository>();
            _userManager = A.Fake<UserManager<UserAccount>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");
        }


        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnSuccess()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var user = new UserAccount { UserName = "TestLogin123", Id = "123" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult(user));

            // Act
            var result = await tokenManager.CreateAccessTokenAsync(user) as ResultT<string>;

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }


        [Theory]
        [InlineData("login", null)]
        [InlineData(null, "id")]
        public async Task CreateAccessTokenAsync_ShouldReturnBadRequestError_WhenDataIsNotValid(string login, string id)
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var user = new UserAccount { UserName = login, Id = id };

            // Act
            var result = await tokenManager.CreateAccessTokenAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.BadRequest);
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnBadRequestError_WhenDataIsNotExists()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);

            // Act
            var result = await tokenManager.CreateAccessTokenAsync(null);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.BadRequest);
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnNotFoundError()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var user = new UserAccount { UserName = "TestLogin123", Id = "123" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Returns(Task.FromResult<UserAccount>(null));

            // Act
            var result = await tokenManager.CreateAccessTokenAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.NotFound);
        }

        [Fact]
        public async Task CreateAccessTokenAsync_ShouldReturnInternalServerError()
        {
            // Arrange
            var tokenManager = new TokenManager(_configuration, _refreshTokenRepository, _userManager);
            var user = new UserAccount { UserName = "TestLogin123", Id = "123" };

            A.CallTo(() => _userManager.FindByIdAsync(user.Id))
                           .Throws(new Exception());

            // Act
            var result = await tokenManager.CreateAccessTokenAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();

            var error = result.Error as Error;
            error.Should().NotBeNull();
            error!.ErrorType.Should().Be(HttpErrorType.InternalServerError);
        }
    }
}
