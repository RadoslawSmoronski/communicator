using Application.Interfaces.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Application.Users.Commands.ChangePassword
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUserService _userService;
        private readonly ILogger<ChangePasswordCommand> _logger;

        public ChangePasswordHandler(IUserService userService, ILogger<ChangePasswordCommand> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
            => await _userService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);
    }
}
