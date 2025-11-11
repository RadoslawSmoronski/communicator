using Application.Common.Authorization;
using Application.Repositories;

namespace Infrastructure.Authorization
{
    public class FriendInvitationAccess : IFriendInvitationAccess
    {
        private IUnitOfWork _unitOfWork;

        public FriendInvitationAccess(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> IsRecipientAsync(Guid userId, Guid invitationId, CancellationToken ct)
        {
            var invitation = await _unitOfWork.FriendshipInvitations.GetById(invitationId);
            return invitation is not null && invitation.RecipientId == userId;
        }

        public async Task<bool> IsUserInvitationParticipantAsync(Guid userId, Guid? invitationId, CancellationToken ct)
        {
            if (invitationId is null)
            {
                var invitations = await _unitOfWork.FriendshipInvitations.GetAllAsync(userId);
                return invitations.Any(x => x.RecipientId == userId || x.SenderId == userId);
            }

            var invitation = await _unitOfWork.FriendshipInvitations.GetById(invitationId.Value);
            return invitation is not null && (invitation.RecipientId == userId || invitation.SenderId == userId);
        }
    }
}
