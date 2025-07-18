using ChatCommunicator.Contracts;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using AutoMapper;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Application.Managers;
using Microsoft.Extensions.Logging;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public abstract class AccountManagerTest
    {
        protected readonly ITokenService _tokenService;
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly SignInManager<UserAccount> _signInManager;
        protected readonly IUserAvatarService _userAvatarService;
        protected readonly IMapper _mapper;
        protected readonly ILogger<AccountManager> _logger;

        protected readonly IAccountManager _accountManager;

        protected readonly UserAccount _sampleUser1;
        protected readonly UserAccount _sampleUser2;
        protected readonly UserAccount _sampleUser3;

        protected readonly RegisterDto _sampleUser1RegisterDto;

        protected const string SAMPLE_STRING_GUID = "12345678-1234-1234-1234-123456789abc";
        protected const string SAMPLE_STRING_EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

        protected AccountManagerTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _tokenService = A.Fake<ITokenService>();
            _signInManager = A.Fake<SignInManager<UserAccount>>();
            _userAvatarService = A.Fake<IUserAvatarService>();
            _logger = A.Fake<ILogger<AccountManager>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configuration.CreateMapper();

            _accountManager = new AccountManager(_userManager, _mapper, _signInManager, _tokenService, _userAvatarService, _logger);    

            _sampleUser1 = new UserAccount { UserName = "User1Login", Id = Guid.NewGuid() };
            _sampleUser2 = new UserAccount { UserName = "User2Login", Id = Guid.NewGuid() };
            _sampleUser3 = new UserAccount { UserName = "User3Login", Id = Guid.NewGuid() };

            _sampleUser1RegisterDto = _mapper.Map<RegisterDto>(_sampleUser1);
            _sampleUser1RegisterDto.Password = "test";
        }
    }
}

