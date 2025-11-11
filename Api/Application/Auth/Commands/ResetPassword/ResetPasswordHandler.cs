using Application.Common.Interfaces.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Result;
using System.Net;
using System.Xml;

namespace Application.Auth.Commands.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly IUserService _userService;

        public ResetPasswordHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var decodedToken = WebUtility.UrlDecode(request.CodedToken);
            return await _userService.ResetPasswordAsync(request.UserId, decodedToken, request.NewPassword);
        }
    }
}
