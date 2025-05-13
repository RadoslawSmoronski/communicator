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
        public Guid Token { get; set; } = Guid.Empty;
        public Guid UserId { get; set; } = Guid.Empty;
        public DateTime Expiration {  get; set; } = DateTime.MinValue;
    }
}
