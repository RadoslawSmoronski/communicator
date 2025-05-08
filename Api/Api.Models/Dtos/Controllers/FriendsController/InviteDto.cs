using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.FriendsController
{
    public class InviteDto
    {
        [Required(ErrorMessage = "SenderId is required.")]
        public Guid SenderId { get; set; } = Guid.Empty;

        [Required(ErrorMessage = "RecipientId is required.")]
        public Guid RecipientId { get; set; } = Guid.Empty;
    }
}
