using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos
{
    public class SimpleUserDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string userName { get; set; } = String.Empty;
    }
}
