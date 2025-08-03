using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Contracts;
using Microsoft.AspNetCore.Identity;
using FakeItEasy;
using AutoMapper;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Application.Services;
using Microsoft.Extensions.Logging;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Chat;

namespace ChatCommunicator.Tests.Managers.ChatManagerTest
{
    public abstract class ChatServiceTest
    {
        protected readonly UserManager<UserAccount> _userManager;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IChatService _chatManager;
        protected readonly IMapper _mapper;
        protected readonly ILogger<ChatService> _logger;

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

        protected ChatServiceTest()
        {
            _userManager = A.Fake<UserManager<UserAccount>>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _logger = A.Fake<ILogger<ChatService>>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configuration.CreateMapper();

            _chatManager = new ChatService(_unitOfWork, _userManager, _mapper, _logger);
            _sampleUser1 = new UserAccount { UserName = "User1Login", Id = Guid.NewGuid() };
            _sampleUser2 = new UserAccount { UserName = "User2Login", Id = Guid.NewGuid() };
            _sampleUser3 = new UserAccount { UserName = "User3Login", Id = Guid.NewGuid() };

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
                Timestamp = DateTime.UtcNow.AddSeconds(-20)
            };

            _sampleMessage2 = new Message()
            {
                ConversationId = _sampleConversation.Id,
                Conversation = _sampleConversation,
                SenderId = _sampleUser2.Id,
                Sender = _sampleUser2,
                Content = "sampleMessage2",
                Timestamp = DateTime.UtcNow.AddSeconds(-10)
            };

            _sampleMessage3 = new Message()
            {
                ConversationId = _sampleConversation.Id,
                Conversation = _sampleConversation,
                SenderId = _sampleUser1.Id,
                Sender = _sampleUser1,
                Content = "sampleMessage3",
                Timestamp = DateTime.UtcNow
            };

            _sampleMessagesList = new List<Message>()
                { _sampleMessage1, _sampleMessage2, _sampleMessage3 };

        }
    }
}

