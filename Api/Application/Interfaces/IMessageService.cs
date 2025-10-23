using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IMessageService
    {
        Task<Result<MessageDto>> SendMessageAsync(Guid userId, Guid conversationId, string content);
        Task<Result<Guid>> SetAndGetUserLastReadMessageAsync(Guid userId, Guid conversationId);
        Task<Result<ExtendedPagedMessagesDto>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid userId, Guid? fromMessageId);
    }
}
