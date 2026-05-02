using Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Result;

namespace Application.Users.Commands.ChangeAvatar
{
    public sealed record ChangeAvatarCommand(Guid UserId, IFormFile File) : IRequest<Result<string>>, IRequireSameUser
    {
        public Guid TargetUserId => UserId;
    }
}
