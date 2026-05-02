using Application.Chats.Queries.GetPagedMessages;

namespace API.Contracts.Chats
{
    public sealed record GetPagedMessagesResponse(
        IEnumerable<MessageReadModel> Messages,
        Guid? LastFriendReadMessageId = null
    );
}
