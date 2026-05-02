using Domain.Entities;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Repositories;
using Infrastructure.Entities;
using Infrastructure.Services;

namespace Infrastructure.UnitTests.Services.FriendshipServiceTests
{
    public abstract class FriendshipServiceTestBase
    {
        protected readonly UserManager<UserAccount> UserManager;
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly ILogger<FriendshipService> Logger;

        protected readonly Guid SampleUser1Id = Guid.NewGuid();
        protected readonly Guid SampleUser2Id = Guid.NewGuid();

        protected readonly UserAccount SampleUserAccount1;
        protected readonly UserAccount SampleUserAccount2;
        
        protected readonly Friendship SampleFriendship;

        protected FriendshipServiceTestBase()
        {
            UserManager = A.Fake<UserManager<UserAccount>>();
            UnitOfWork = A.Fake<IUnitOfWork>();
            Logger = A.Fake<ILogger<FriendshipService>>();

            SampleUserAccount1 = new UserAccount
            {
                Id = SampleUser1Id,
                UserName = "User1",
                EmailConfirmed = true
            };

            SampleUserAccount2 = new UserAccount
            {
                Id = SampleUser2Id,
                UserName = "User2",
                EmailConfirmed = true
            };

            SampleFriendship = new Friendship
            {
                Id = Guid.NewGuid(),
                User1Id = SampleUser1Id,
                User2Id = SampleUser2Id
            };
            
        }

        protected FriendshipService CreateService()
        {
            return new FriendshipService(UserManager, UnitOfWork, Logger);
        }
    }
}