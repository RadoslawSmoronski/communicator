using Api.Models;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private UserManager<UserAccount> _userManager;

        public UserController(UserManager<UserAccount> userManager)
        {
            _userManager = userManager;
        }

    }
}
