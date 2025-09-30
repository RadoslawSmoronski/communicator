using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UploadAvatarDto
    {
        [Required]
        public required IFormFile File { get; set; }
    }
}
