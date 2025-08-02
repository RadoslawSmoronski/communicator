using System;

namespace ChatCommunicator.Contracts.Dtos.Friendships;

public class AcceptFriendshipInviteDto
{
    public required Guid FriendshipId { get; set; }
    public required Guid ConversationId { get; set; }
}
