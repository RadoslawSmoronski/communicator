using Application.Common.Authorization;
using Application.Repositories;

namespace Infrastructure.Authorization
{
    public class ChatAccess(IUnitOfWork unitOfWork) : IChatAccess
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> IsParticipantAsync(Guid userId, Guid chatId, CancellationToken ct)
        {
            var chat = await _unitOfWork.Conversations.GetConversationByIdAsync(chatId);
            return chat is not null && (chat.User1Id == userId || chat.User2Id == userId);
        }
    }
}
