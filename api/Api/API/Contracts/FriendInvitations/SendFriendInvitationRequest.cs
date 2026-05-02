using System.ComponentModel.DataAnnotations;

namespace API.Contracts.FriendInvitations
{
    public sealed record SendFriendInvitationRequest
    {
        [Required(ErrorMessage = "SenderId is required.")]
        public Guid SenderId { get; init; } = default!;

        [Required(ErrorMessage = "RecipientId is required.")]
        public Guid RecipientId { get; init; } = default!;
    }
}
