using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = String.Empty;
        public string UserId { get; set; } = String.Empty;
        public DateTime Expiration {  get; set; } = DateTime.MinValue;
    }
}
