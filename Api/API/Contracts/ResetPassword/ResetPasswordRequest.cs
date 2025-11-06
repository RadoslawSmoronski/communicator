using System.ComponentModel.DataAnnotations;

namespace API.Contracts.ResetPassword
{
    public sealed record ResetPasswordRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; init; } = default!;

        [Required(ErrorMessage = "CodedToken is required.")]
        public string CodedToken { get; init; } = default!;

        [Required(ErrorMessage = "NewPassword is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters long.")]
        public string NewPassword { get; init; } = default!;
    }
}
