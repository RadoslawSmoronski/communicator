using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ChangeUsernameDto
    {
        [Required]
        public required string NewUsername { get; set; }
    }
}
