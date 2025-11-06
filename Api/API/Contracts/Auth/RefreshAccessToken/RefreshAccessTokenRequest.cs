using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Auth.RefreshAccessToken
{
    public sealed record RefreshAccessTokenRequest
    {
        [Required(ErrorMessage = "RefreshToken is required.")]
        public Guid RefreshToken { get; init; } = default!;
    }
}
