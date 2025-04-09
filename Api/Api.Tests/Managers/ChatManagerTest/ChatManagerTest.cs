using Api.Data.UnitOfWork;
using Api.Managers;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using Api.Managers.Interfaces;
using Api.Models.Chat;
using AutoMapper;

namespace Api.Tests.Managers.ChatManagerTest
{
    public abstract class ChatManagerTest
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IChatManager _chatManager;
        protected readonly IFriendsManager _friendsManager;
        protected readonly IMapper _mapper;

        protected readonly UserAccount _sampleUser1;
        protected readonly UserAccount _sampleUser2;
        protected readonly UserAccount _sampleUser3;

        protected readonly Conversation _sampleConversation;
        protected readonly Conversation _sampleConversation2;
        protected readonly IEnumerable<Conversation> _sampleConversationsList;

        protected readonly Message _sampleMessage1;
        protected readonly Message _sampleMessage2;
        protected readonly Message _sampleMessage3;
        protected readonly IEnumerable<Message> _sampleMessagesList;

        protected ChatManagerTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _friendsManager = A.Fake<IFriendsManager>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configuration.CreateMapper();

            _chatManager = new ChatManager(_unitOfWork, _userManager, _friendsManager, _mapper);
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

            _sampleMessage1 = new Message()
            {
                ConversationId = _sampleConversation.Id,
                Conversation = _sampleConversation,
                SenderId = _sampleUser1.Id,
                Sender = _sampleUser1,
                Content = "sampleMessage1",
                IsRead = true
            };

            _sampleMessage2 = new Message()
            {
                ConversationId = _sampleConversation.Id,
                Conversation = _sampleConversation,
                SenderId = _sampleUser2.Id,
                Sender = _sampleUser2,
                Content = "sampleMessage2",
                IsRead = true
            };

            _sampleMessage3 = new Message()
            {
                ConversationId = _sampleConversation.Id,
                Conversation = _sampleConversation,
                SenderId = _sampleUser1.Id,
                Sender = _sampleUser1,
                Content = "sampleMessage3",
                IsRead = true
            };

            _sampleMessagesList = new List<Message>()
                { _sampleMessage1, _sampleMessage2, _sampleMessage3 };

        }
    }
}

