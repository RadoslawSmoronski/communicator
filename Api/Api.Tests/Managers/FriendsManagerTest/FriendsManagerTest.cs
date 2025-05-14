using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Tests.Managers.FriendsManagerTest
{
    public abstract class FriendsManagerTest
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IUnitOfWork _unitOfWork;

        protected readonly FriendsManager _friendsManager;

        protected readonly UserAccount _sampleSenderUser;
        protected readonly UserAccount _sampleRecipientUser;
        protected readonly UserAccount _sampleUser;

        protected const string SAMPLE_STRING_GUID = "12345678-1234-1234-1234-123456789abc";
        protected const string SAMPLE_STRING_EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

        protected FriendsManagerTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            _friendsManager = new FriendsManager(_userManager, _unitOfWork);
            _sampleSenderUser = new UserAccount { UserName = "senderUserLogin", Id = Guid.NewGuid() };
            _sampleRecipientUser = new UserAccount { UserName = "recipientUserLogin", Id = Guid.NewGuid() };
            _sampleUser = new UserAccount { UserName = "sampleUserLogin", Id = Guid.NewGuid() };
        }
    }
}