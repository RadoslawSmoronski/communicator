using Application.Common.Authorization;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Security;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICurrentUser _currentUser;
    //private readonly IChatAccess _chatAccess;
    //private readonly IFriendInvitationAccess _invitationAccess;
    //private readonly IFriendshipAccess _friendshipAccess;

    public AuthorizationBehavior(
        ICurrentUser currentUser)
        //IChatAccess chatAccess,
        //IFriendInvitationAccess invitationAccess,
        //IFriendshipAccess friendshipAccess)
    {
        _currentUser = currentUser;
        //_chatAccess = chatAccess;
        //_invitationAccess = invitationAccess;
        //_friendshipAccess = friendshipAccess;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAllowAnonymous || _currentUser.IsInRole("Admin"))
            return await next();

        var currentUserId = _currentUser.Id ?? throw new UnauthorizedAccessException();

        if (request is IRequireSameUser sameUser && !(currentUserId == sameUser.TargetUserId))
        {
            throw new ForbiddenException("You are not allowed to act on another user's resource.");
        }

        //if (request is IRequireChatMembership chatReq)
        //{
        //    var ok = await _chatAccess.IsMemberAsync(currentUserId, chatReq.ChatId, cancellationToken);
        //    if (!ok) throw new NotFoundException("Chat", chatReq.ChatId);
        //}

        //if (request is IRequireInvitationRecipient invReq)
        //{
        //    var ok = await _invitationAccess.IsRecipientAsync(currentUserId, invReq.InvitationId, cancellationToken);
        //    if (!ok) throw new NotFoundException("FriendInvitation", invReq.InvitationId);
        //}

        //if (request is IRequireFriendshipParticipant frReq)
        //{
        //    var ok = await _friendshipAccess.IsParticipantAsync(currentUserId, frReq.FriendshipId, cancellationToken);
        //    if (!ok) throw new NotFoundException("Friendship", frReq.FriendshipId);
        //}

        return await next();
    }
}