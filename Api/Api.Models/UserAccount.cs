using ChatCommunicator.Models.Friendship;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Models
{
    public class UserAccount : IdentityUser<Guid>
    {
    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
