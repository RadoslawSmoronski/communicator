using Api.Data.IRepository;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IPendingFriendshipRepository _pendingFriendshipRepository;

        public FriendsController(UserManager<UserAccount> userManager, IPendingFriendshipRepository pendingFriendshipRepository)
        {
            _userManager = userManager;
            _pendingFriendshipRepository = pendingFriendshipRepository;
        }

    }
}
