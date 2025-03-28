using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Managers;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public abstract class FriendsManagerTest
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IUnitOfWork _unitOfWork;

        protected readonly FriendsManager _friendsManager;
        protected readonly UserAccount _sampleUser;
        protected readonly UserAccount _sampleUser2;

        protected FriendsManagerTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            _friendsManager = new FriendsManager(_userManager, _unitOfWork);
            _sampleUser = new UserAccount { UserName = "senderUserLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab254" };
            _sampleUser2 = new UserAccount { UserName = "recipientUserLogin", Id = "c9fbf188-e309-48c9-811d-7d5be45ab255" };
        }
    }
}
