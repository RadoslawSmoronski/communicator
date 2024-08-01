using FakeItEasy;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Api.Models;
using Api.Controllers;

namespace Api.Tests.Controllers
{
    public class UserControllerTest
    {
        [Fact]
        public void Register_ShouldReturnTrue_WhenCalledWithValidParameters()
        {
            // Arrange
            var userController = A.Fake<UserController>();
            var registerDto = new RegisterDto(UserName: "login", Password: "password123");

            // Act
            var result = userController.Register(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RegisterDto>();
            result.Should().Be().EquivalentTo(registerDto);
        }
    }
}
