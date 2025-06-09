using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Contracts.Dtos.Service
{
    public class RefreshAccessTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public Guid RefreshToken { get; set; } = Guid.Empty;
    }
}
