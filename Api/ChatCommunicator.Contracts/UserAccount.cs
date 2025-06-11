using ChatCommunicator.Contracts.Friendship;
using Microsoft.AspNetCore.Identity;

namespace ChatCommunicator.Contracts
{
    public class UserAccount : IdentityUser<Guid>
    {
    }

    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
