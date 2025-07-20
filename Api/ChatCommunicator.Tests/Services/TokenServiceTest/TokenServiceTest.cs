using ChatCommunicator.Application.Services;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Infrastructure.UnitOfWork;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ChatCommunicator.Infrastructure.Models;

namespace ChatCommunicator.Tests.Services.TokenServiceTest
{
    public abstract class TokenServiceTest
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IConfiguration _configuration;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger<TokenService> _logger;

        protected readonly ITokenService _tokenService;
        protected readonly Guid _sampleUserId;
        protected readonly Guid _sampleRefreshToken;
        protected readonly UserAccount _sampleUserAccount;

        protected TokenServiceTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _logger = A.Fake<ILogger<TokenService>>();

            _configuration = A.Fake<IConfiguration>();
            A.CallTo(() => _configuration["JWT:SigningKey"]).Returns("sgdfgfdgdrt45345klopdgdfge543532fdgdbfdisjdhdgdfgfdvgfdgdggpdvbl3gr4t");
            A.CallTo(() => _configuration["JWT:Issuer"]).Returns("your-issuer");
            A.CallTo(() => _configuration["JWT:Audience"]).Returns("your-audience");

            _unitOfWork = A.Fake<IUnitOfWork>();

            _tokenService = new TokenService(_configuration, _userManager, _unitOfWork, _logger);
            _sampleUserId = Guid.NewGuid();
            _sampleRefreshToken = Guid.NewGuid();
            _sampleUserAccount = new UserAccount { UserName = "TestLogin123", Id = Guid.NewGuid() };
        }
    }
}
