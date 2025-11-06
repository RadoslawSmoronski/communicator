using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Auth.RequestPasswordReset
{
    public sealed record RequestPasswordResetRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; init; } = default!;
    }
}
