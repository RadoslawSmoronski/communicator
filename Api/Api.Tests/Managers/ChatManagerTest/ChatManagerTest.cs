using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using Api.Managers.Interfaces;
using Api.Models.Chat;

namespace Api.Tests.Managers.ChatManagerTest
{
    public abstract class ChatManagerTest
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IUnitOfWork _unitOfWork;

        protected readonly IChatManager _chatManager;
        protected readonly IFriendsManager _friendsManager;

        protected readonly UserAccount _sampleUser1;
        protected readonly UserAccount _sampleUser2;
        protected readonly UserAccount _sampleUser3;
        protected readonly Conversation _sampleConversation;
        protected readonly Conversation _sampleConversation2;
        protected readonly IEnumerable<Conversation> _sampleConversationsList;

        protected ChatManagerTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _friendsManager = A.Fake<IFriendsManager>();

            _chatManager = new ChatManager(_unitOfWork, _userManager, _friendsManager);
            _sampleUser1 = new UserAccount { UserName = "User1Login", Id = "c9fbf188-e309-48c9-811d-7d5be45ab254" };
            _sampleUser2 = new UserAccount { UserName = "User2Login", Id = "c9fbf188-e309-48c9-811d-7d5be45ab255" };
            _sampleUser3 = new UserAccount { UserName = "User3Login", Id = "c9fbf188-e309-48c9-811d-7d5be45ab256" };

            _sampleConversation = new Conversation()
            {
                User1Id = _sampleUser1.Id,
                User2Id = _sampleUser2.Id,
                User1 = _sampleUser1,
                User2 = _sampleUser2
            };

            _sampleConversation2 = new Conversation()
            {
                User1Id = _sampleUser3.Id,
                User2Id = _sampleUser1.Id,
                User1 = _sampleUser3,
                User2 = _sampleUser1
            };


            _sampleConversationsList = new List<Conversation>
                { _sampleConversation, _sampleConversation2};

        }
    }
}

