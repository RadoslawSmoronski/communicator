using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Auth.ConfirmEmail
{
    public sealed record ConfirmEmailRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; init; } = default!;

        [Required(ErrorMessage = "ConfirmationToken is required.")]
        public string ConfirmationToken { get; init; } = default!;
    }
}
