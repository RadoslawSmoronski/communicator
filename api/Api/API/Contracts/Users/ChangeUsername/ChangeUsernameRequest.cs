using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Users.ChangeUsername
{
    public sealed record ChangeUsernameRequest
    {
        [Required(ErrorMessage = "New username is required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 25 characters.")]
        public string NewUsername { get; init; } = default!;
    }
}
