using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.FriendsController
{
    public class DecelineInviteDto
    {
        [Required]
        [StringLength(36, MinimumLength = 36, ErrorMessage = "SenderId must be exactly 36 characters long.")]
        public string SenderId { get; set; } = string.Empty;
        [Required]
        [StringLength(36, MinimumLength = 36, ErrorMessage = "SenderId must be exactly 36 characters long.")]
        public string RecipientId { get; set; } = string.Empty;
    }
}
