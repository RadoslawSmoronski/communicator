using ChatCommunicator.Contracts;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using AutoMapper;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Application.Managers;
using Microsoft.Extensions.Logging;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Options;

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
        protected readonly IEmailService _emailService;
        protected readonly IOptions<ConfirmEmailMessageSettings> _confirmEmailMessageSettings;

        protected readonly IAccountManager _accountManager;

        protected readonly UserAccount _sampleUser1;
        protected readonly UserAccount _sampleUser2;
        protected readonly UserAccount _sampleUser3;
        protected readonly UserAccount _sampleUserWithAvatar;

        protected readonly RegisterDto _sampleUser1RegisterDto;
        protected readonly RegisteredDto _sampleRegistredDtoUser1;

        protected const string SAMPLE_STRING_GUID = "12345678-1234-1234-1234-123456789abc";
        protected const string SAMPLE_STRING_EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

        protected AccountManagerTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _tokenService = A.Fake<ITokenService>();
            _signInManager = A.Fake<SignInManager<UserAccount>>();
            _userAvatarService = A.Fake<IUserAvatarService>();
            _logger = A.Fake<ILogger<AccountManager>>();
            _emailService = A.Fake<IEmailService>();

            var testConfirmEmailMessageSettings = new ConfirmEmailMessageSettings
            {
                Title = "test",
                Content = "Test email content: [address]",
                Address = "test"
            };

            _confirmEmailMessageSettings = A.Fake<IOptions<ConfirmEmailMessageSettings>>();
            A.CallTo(() => _confirmEmailMessageSettings.Value).Returns(testConfirmEmailMessageSettings);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configuration.CreateMapper();

            _accountManager = new AccountManager(_userManager, _mapper, _signInManager, _tokenService, _emailService, _userAvatarService, _logger, _confirmEmailMessageSettings);    

            _sampleUser1 = new UserAccount { UserName = "User1Login", Id = Guid.NewGuid(), Email = "test@test.com"};
            _sampleUser2 = new UserAccount { UserName = "User2Login", Id = Guid.NewGuid() };
            _sampleUser3 = new UserAccount { UserName = "User3Login", Id = Guid.NewGuid() };
            _sampleUserWithAvatar = new UserAccount { UserName = "UserWithAvatar", Id = Guid.NewGuid(), AvatarUrl = "avatar.png"};

            _sampleUser1RegisterDto = _mapper.Map<RegisterDto>(_sampleUser1);
            _sampleUser1RegisterDto.Password = "test";

            _sampleRegistredDtoUser1 = _mapper.Map<RegisteredDto>(_sampleUser1);
        }
    }
}

