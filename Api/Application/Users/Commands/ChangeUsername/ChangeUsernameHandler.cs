using Application.Common.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Result;
using System.Numerics;

namespace Application.Users.Commands.ChangeUsername
{
    public class ChangeUsernameHandler : IRequestHandler<ChangeUsernameCommand, Result<string>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<ChangeUsernameCommand> _logger;

        public ChangeUsernameHandler(IUserService userService, ILogger<ChangeUsernameCommand> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
            => await _userService.ChangeUsernameAsync(request.UserId, request.NewPassword);
    }
}
