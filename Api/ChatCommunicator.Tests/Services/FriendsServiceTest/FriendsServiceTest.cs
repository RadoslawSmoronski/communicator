using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatCommunicator.Application.Managers;
using Microsoft.Extensions.Logging;
using ChatCommunicator.Contracts.Friendship;

namespace ChatCommunicator.Tests.Services.FriendsManagerTest
{
    public abstract class FriendsServiceTest
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger<FriendsService> _logger;

        protected readonly FriendsService _friendsManager;

        protected readonly UserAccount _sampleSenderUser;
        protected readonly UserAccount _sampleRecipientUser;
        protected readonly UserAccount _sampleUser;

        protected readonly FriendshipInvitation _sampleFriendshipInvitation;

        protected const string SAMPLE_STRING_GUID = "12345678-1234-1234-1234-123456789abc";
        protected const string SAMPLE_STRING_EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

        protected FriendsServiceTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<FriendsService>>();

            _friendsManager = new FriendsService(_userManager, _unitOfWork, _logger);
            _sampleSenderUser = new UserAccount { UserName = "senderUserLogin", Id = Guid.NewGuid() };
            _sampleRecipientUser = new UserAccount { UserName = "recipientUserLogin", Id = Guid.NewGuid() };
            _sampleUser = new UserAccount { UserName = "sampleUserLogin", Id = Guid.NewGuid() };
            _sampleFriendshipInvitation = new FriendshipInvitation { SenderId = _sampleSenderUser.Id, RecipientId = _sampleRecipientUser.Id, SenderUser = _sampleSenderUser, RecipientUser = _sampleRecipientUser };
        }
    }
}