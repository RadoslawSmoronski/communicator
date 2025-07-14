using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChatCommunicator.Contracts.Dtos.Controllers.UserController
{
    public class UploadAvatarDto
    {
        [Required]
        [FromForm(Name = "file")]
        public IFormFile? File { get; set; }
    }
}
