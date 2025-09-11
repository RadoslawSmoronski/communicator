using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity
{
    public class UserAccount : IdentityUser<Guid>
    {
        public string? AvatarUrl { get; set; }

    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
