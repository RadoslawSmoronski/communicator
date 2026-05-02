using Application.Contracts.Chat;
using Domain.Entities;
using Shared.Result;

namespace Application.Common.Interfaces
{
    public interface IMessageService
    {
        Task<Result<MessageReceivedEvent>> SendMessageAsync(Guid userId, Guid conversationId, string content);
        Task<Result<Guid>> SetAndGetUserLastReadMessageAsync(Guid userId, Guid conversationId);
        Task<Result<List<Message>>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid userId, Guid? fromMessageId);
    }
}
