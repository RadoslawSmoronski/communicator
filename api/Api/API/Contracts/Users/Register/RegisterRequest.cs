using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Users.Register
{
    public sealed record RegisterRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; init; } = default!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters long.")]
        public string Password { get; init; } = default!;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 25 characters.")]
        public string Username { get; init; } = default!;
    }
}