using Application.Common.Authorization;
using Application.Common.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Shared.Result;

namespace Application.Chats.Queries.GetPagedMessages
{
    public class GetPagedMessagesHandler : IRequestHandler<GetPagedMessagesQuery, Result<GetPagedMessagesReadModel>>
    {
        private readonly IMessageService _messageService;
        private readonly IMapper _mapper;
        private readonly IUsersConnectionService _usersConnectionService;
        private readonly IConversationService _conversationService;

        public GetPagedMessagesHandler(IMessageService messageService, IMapper mapper, IUsersConnectionService usersConnectionService, IConversationService conversationService)
        {
            _messageService = messageService;
            _mapper = mapper;
            _usersConnectionService = usersConnectionService;
            _conversationService = conversationService;
        }

        public async Task<Result<GetPagedMessagesReadModel>> Handle(GetPagedMessagesQuery request, CancellationToken cancellationToken)
        {
            var messagesResult = await _messageService.GetPagedMessagesFromMessageIdAsync(request.ConversationId, request.UserId, request.FromMessageId);
            if (!messagesResult.IsSuccess)
                return messagesResult.Error!;
            var messages = _mapper.Map<List<MessageReadModel>>(messagesResult.Value);

            if (messages.Count < 1)
                return new GetPagedMessagesReadModel
                (
                    Messages: messages,
                    LastFriendReadMessageId: null,
                    RecipientConnectionsId: null,
                    UserReadMessageId: null
                );

            var conversationResult = await _conversationService.GetByIdAsync(request.ConversationId);
            if (!conversationResult.IsSuccess)
                return conversationResult.Error!;
            var conversation = conversationResult.Value;

            var lastRead = conversation.User1Id == request.UserId
                ? conversation.User2LastReadMessageId
                : conversation.User1LastReadMessageId;

            var setUserLastMessageResult = await _messageService.SetAndGetUserLastReadMessageAsync(request.UserId, request.ConversationId);
            if (!setUserLastMessageResult.IsSuccess)
                return setUserLastMessageResult.Error!;

            var userReadMessageId = setUserLastMessageResult.Value;

            var recipientId = conversation.User1Id == request.UserId
                ? conversation.User2Id
                : conversation.User1Id;
            var recipientConnectionsId = _usersConnectionService.GetUserConnectionsId(recipientId);

            return new GetPagedMessagesReadModel
            (
                Messages: messages,
                LastFriendReadMessageId: lastRead,
                RecipientConnectionsId: recipientConnectionsId,
                UserReadMessageId: userReadMessageId
            );
        }
    }
}
