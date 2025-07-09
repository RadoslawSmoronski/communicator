//using ChatCommunicator.Contracts;
//using FakeItEasy;
//using Microsoft.AspNetCore.Identity;
//using ChatCommunicator.Shared.Result;
//using FluentAssertions;
//using Microsoft.Extensions.Configuration;
//using ChatCommunicator.Infrastructure.UnitOfWork;
//using System.Linq.Expressions;
//using ChatCommunicator.Application.Services.Interfaces;
//using ChatCommunicator.Application.Services;

//namespace ChatCommunicator.Tests.Services.TokenServiceTest
//{
//    public class RemoveExpiredRefreshTokensAsyncTest
//    {
//        private readonly UserManager<UserAccount> _userManager;
//        private readonly IConfiguration _configuration;
//        private readonly IUnitOfWork _unitOfWork;

//        private readonly ITokenService _TokenService;

//        public RemoveExpiredRefreshTokensAsyncTest()
//        {
//            _userManager = A.Fake<UserManager<UserAccount>>();

//            _configuration = A.Fake<IConfiguration>();
//            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
//            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
//            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

//            _unitOfWork = A.Fake<IUnitOfWork>();

//            _TokenService = new TokenService(_configuration, _userManager, _unitOfWork);
//        }


//        [Fact]
//        public async Task RemoveExpiredRefreshTokensAsync_ShouldReturnSuccess()
//        {
//            //Arrange
//            IEnumerable<RefreshToken> refreshTokenList = new List<RefreshToken>()
//            {
//                new RefreshToken(),
//                new RefreshToken()
//            };

//            A.CallTo(() => _unitOfWork.RefreshTokens.WhereAsync(A<Expression<Func<RefreshToken, bool>>>._))
//                           .Returns(Task.FromResult(refreshTokenList));

//            // Act
//            var result = await _TokenService.RemoveExpiredRefreshTokensAsync() as ResultT<int>;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeTrue();
//            result.Value.Should().Be(2);
//        }

//        [Fact]
//        public async Task RemoveExpiredRefreshTokensAsync_ShouldReturnNotFound()
//        {
//            //Arrange
//            IEnumerable<RefreshToken> refreshTokenList = new List<RefreshToken>();

//            A.CallTo(() => _unitOfWork.RefreshTokens.WhereAsync(A<Expression<Func<RefreshToken, bool>>>._))
//                           .Returns(Task.FromResult(refreshTokenList));

//            // Act
//            var result = await _TokenService.RemoveExpiredRefreshTokensAsync() as ResultT<int>;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(ErrorType.Failure);
//        }

//        [Fact]
//        public async Task RemoveExpiredRefreshTokensAsync_ShouldReturnInternalServerError()
//        {
//            // Arrange
//            A.CallTo(() => _unitOfWork.RefreshTokens.WhereAsync(A<Expression<Func<RefreshToken, bool>>>._))
//                .ThrowsAsync(new Exception());

//            // Act
//            var result = await _TokenService.RemoveExpiredRefreshTokensAsync() as ResultT<int>;

//            // Assert
//            result.Should().NotBeNull();
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().NotBeNull();

//            var error = result.Error! as Error;
//            error.ErrorType.Should().Be(ErrorType.Unknown);
//        }
//    }
//}
