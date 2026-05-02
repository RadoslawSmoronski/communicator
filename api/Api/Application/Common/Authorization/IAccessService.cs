namespace Application.Common.Authorization;

public interface IChatAccess
{
    Task<bool> IsParticipantAsync(Guid userId, Guid chatId, CancellationToken ct);
}

public interface IFriendInvitationAccess
{
    Task<bool> IsRecipientAsync(Guid userId, Guid invitationId, CancellationToken ct);
    Task<bool> IsUserInvitationParticipantAsync(Guid userId, Guid? invitationId, CancellationToken ct);
}

public interface IFriendshipAccess
{
    Task<bool> IsParticipantAsync(Guid userId, Guid friendshipId, CancellationToken ct);
}