using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Contracts
{
    public class UserAccount : IdentityUser<Guid>
    {
        public string? AvatarUrl { get; set; }
    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
