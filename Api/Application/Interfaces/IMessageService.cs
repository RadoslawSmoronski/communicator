using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IMessageService
    {
        Task<Result<MessageDto>> SendMessageAsync(Guid userId, Guid conversationId, string content);
    }
}
