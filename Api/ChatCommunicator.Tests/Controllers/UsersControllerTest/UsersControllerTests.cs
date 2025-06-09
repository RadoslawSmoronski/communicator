using ChatCommunicator.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using ChatCommunicator.API.Controllers;

namespace ChatCommunicator.Tests.Controllers.UsersControllerTest
{
    public abstract class UsersControllerTests
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IMapper _mapper;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected readonly UsersController _usersController;
        protected readonly UserAccount _sampleUserAccount;
        protected readonly List<UserAccount> _sampleUsersList;

        protected UsersControllerTests()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _httpContextAccessor = A.Fake<IHttpContextAccessor>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _usersController = new UsersController(_userManager, _mapper, _httpContextAccessor);

            _sampleUserAccount = new UserAccount { UserName = "TestLogin123", Id = Guid.NewGuid(), Email = "user4@example.com" };

            _sampleUsersList = new List<UserAccount>
            {
                new UserAccount { Id = Guid.NewGuid(), UserName = "John1", Email = "user1@example.com" },
                new UserAccount { Id = Guid.NewGuid(), UserName = "John2", Email = "user2@example.com" },
                new UserAccount { Id = Guid.NewGuid(), UserName = "AdmJoh3", Email = "user3@example.com" },
                _sampleUserAccount
            };
        }
    }
}
