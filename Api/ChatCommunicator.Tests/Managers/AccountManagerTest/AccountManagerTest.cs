using ChatCommunicator.Contracts;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using AutoMapper;
using ChatCommunicator.Application.Managers.Interfaces;

namespace ChatCommunicator.Tests.Managers.AccountManagerTest
{
    public abstract class AccountManagerTest
    {
        protected readonly IAccountManager _accountManager;

        protected readonly UserManager<UserAccount> _userManager; 
        protected readonly IMapper _mapper;

        protected readonly UserAccount _sampleUser1;
        protected readonly UserAccount _sampleUser2;
        protected readonly UserAccount _sampleUser3;

        protected AccountManagerTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configuration.CreateMapper();

            _sampleUser1 = new UserAccount { UserName = "User1Login", Id = Guid.NewGuid() };
            _sampleUser2 = new UserAccount { UserName = "User2Login", Id = Guid.NewGuid() };
            _sampleUser3 = new UserAccount { UserName = "User3Login", Id = Guid.NewGuid() };
        }
    }
}

