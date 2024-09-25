using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UsersController.GetUsers
{
    public class GetUsersResponseOkDto
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<GetUsersUserResponseDto> Users { get; set; } = new List<GetUsersUserResponseDto>();
    }
}
