using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Users.ChangePassword
{
    public sealed record ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters long.")]
        public string OldPassword { get; init; } = default!;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters long.")]
        public string NewPassword { get; init; } = default!;
    }
}
