using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Chats.Queries.GetPagedMessages
{
    public record GetPagedMessagesQuery(Guid ConversationId, Guid UserId, Guid? FromMessageId) : IRequest<Result<GetPagedMessagesReadModel>>, IRequireChatParticipant
    {
        public Guid ChatId => ConversationId;
    }
}
