using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RequestPasswordResetDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }
    }
}
