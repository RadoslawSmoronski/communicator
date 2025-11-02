namespace Application.Common.Security;


public interface IAllowAnonymous { }
public interface IRequireSameUser
{
    Guid TargetUserId { get; }
}

public interface IRequireChatMembership
{
    Guid ChatId { get; }
}

public interface IRequireInvitationRecipient
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