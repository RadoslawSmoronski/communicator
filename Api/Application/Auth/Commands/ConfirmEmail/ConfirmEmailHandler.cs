using Application.Common.Interfaces.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Result;
using System.Net;

namespace Application.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailHandler(IUserService userService) : IRequestHandler<ConfirmEmailCommand, Result>
    {
        private readonly IUserService _userService = userService;

        public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
            => await _userService.ConfirmEmailAsync(request.UserId, request.ConfirmationToken);
    }
}