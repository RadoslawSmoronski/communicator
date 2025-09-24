using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RefreshAccessTokenDto
    {
        [Required(ErrorMessage = "RefreshToken is required.")]
        public required Guid RefreshToken { get; set; }
    }
}
