using Api.Models.Friendship;
using Microsoft.AspNetCore.Identity;

namespace Api.Models
{
    public class UserAccount : IdentityUser<Guid>
    {
    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
