namespace Application.Common.Security;


public interface IAllowAnonymous { }
public interface IRequireSameUser
{
    Guid TargetUserId { get; }
}

public interface IRequireChatParticipant
{
    Guid ChatId { get; }
}

public interface IRequireFriendInvitationRecipient
{
    Guid FriendInvitationId { get; }
}

public interface IRequireInvitationParticipant
{
    Guid? FriendInvitationId { get; }
}

public interface IRequireFriendshipParticipant
{
    Guid FriendshipId { get; }
}