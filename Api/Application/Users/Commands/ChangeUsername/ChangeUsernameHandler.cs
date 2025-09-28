using Application.Common.Interfaces;
using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.ChangeUsername
{
    public class ChangeUsernameHandler : IRequestHandler<ChangeUsernameCommand, Result<string>>
    {
        private readonly IUserService _userService;

        public ChangeUsernameHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<string>> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                return Error.Unauthorized("", "");
            }

            //var result = await _userService.ChangeNameAsync(user, newUsername);

            //if (result.Succeeded)
            //{
            //    return newUsername;
            //}

            return "test";

        }
    }
}
