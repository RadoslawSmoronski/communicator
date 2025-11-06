using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Auth.Login
{
    public sealed record LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; init; } = default!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; init; } = default!;
    }
}
