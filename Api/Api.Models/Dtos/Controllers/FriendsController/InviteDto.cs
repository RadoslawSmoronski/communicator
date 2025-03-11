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
        [RegularExpression(@"^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$",
        ErrorMessage = "SenderId not valid format.")]
        public string SenderId { get; set; } = string.Empty;

        [Required(ErrorMessage = "RecipientId is required.")]
        [RegularExpression(@"^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$",
        ErrorMessage = "RecipientId not valid format.")]
        public string RecipientId { get; set; } = string.Empty;
    }
}
