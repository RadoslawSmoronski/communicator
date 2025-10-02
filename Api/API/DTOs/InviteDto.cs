using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class InviteDto
    {
        [Required(ErrorMessage = "SenderId is required.")]
        public required Guid SenderId { get; set; }

        [Required(ErrorMessage = "RecipientId is required.")]
        public required Guid RecipientId { get; set; }
    }
}
