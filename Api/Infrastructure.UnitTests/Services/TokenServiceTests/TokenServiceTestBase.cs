using Application.Common.Settings;
using Application.Repositories;
using Domain.Entities;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Infrastructure.Services;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.UnitTests.Services.TokenServiceTests
{
    public abstract class TokenServiceTestBase
    {
        protected readonly UserManager<UserAccount> UserManager;
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly ILogger<TokenService> Logger;
        
        protected readonly Guid SampleUserId = Guid.NewGuid();
        protected readonly UserAccount SampleUserAccount;
        protected readonly Guid SampleTokenId = Guid.NewGuid();
        protected readonly RefreshToken SampleRefreshToken;
        
        protected readonly JWTTokenSettings AccessTokenSettings = new()
        {
            SigningKey = "test_signing_key_which_is_long_enough_12345_test_test_test_great_tests",
            AccessTokenLifeInSeconds = 3600,
            Issuer = "test-issuer",
            Audience = "test-audience"
        };

        protected readonly RefreshTokenSettings RefreshTokenSettings = new()
        {
            RefreshTokenLifeInSeconds = 7 * 24 * 3600,
            RefreshTokenCleanUpIntercalInSeconds = 24 * 3600
        };

        protected TokenServiceTestBase()
        {
            UserManager = A.Fake<UserManager<UserAccount>>();
            UnitOfWork = A.Fake<IUnitOfWork>();
            Logger = A.Fake<ILogger<TokenService>>();

            SampleUserAccount = new UserAccount { Id = SampleUserId, UserName = "tester" };
            SampleRefreshToken = new RefreshToken
            {
                UserId = SampleUserId,
                Token = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            //A.CallTo(() => UnitOfWork.RefreshTokens).Returns(RefreshTokenRepo);
            //A.CallTo(() => UnitOfWork.SaveAsync()).Returns(Task.CompletedTask);
        }

        protected TokenService CreateService()
        {
            var accessOptions = Options.Create(AccessTokenSettings);
            var refreshOptions = Options.Create(RefreshTokenSettings);
            return new TokenService(UserManager, accessOptions, refreshOptions, Logger, UnitOfWork);
        }
    }
}