using Application.Common.Interfaces.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Result;
using System.Net;

namespace Application.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result>
    {
        private IUserService _userService;

        public ConfirmEmailHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
            => await _userService.ConfirmEmailAsync(request.UserId, request.ConfirmationToken);
    }
}