namespace Application.Common.Authorization;

public interface IChatAccess
{
    Task<bool> IsMemberAsync(Guid userId, Guid chatId, CancellationToken ct);
    Task<bool> IsOwnerAsync(Guid userId, Guid chatId, CancellationToken ct); // optional
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