using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities
{
    public class UserAccount : IdentityUser<Guid>
    {
        public string? AvatarUrl { get; set; }

    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
