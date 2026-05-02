using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Users.UploadAvatar
{
    public sealed record UploadAvatarRequest
    {
        [Required(ErrorMessage = "Avatar file is required.")]
        public IFormFile File { get; init; } = default!;
    }
}
