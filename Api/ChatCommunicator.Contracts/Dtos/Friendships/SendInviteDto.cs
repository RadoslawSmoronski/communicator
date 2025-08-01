using System;

namespace ChatCommunicator.Contracts.Dtos.Friendships;

public class SendInviteDto
{
    public required Guid FriendshipInvitationId { get; set; }
}
