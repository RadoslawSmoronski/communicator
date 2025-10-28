using Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Result;

namespace Application.Users.Commands.UploadAvatar
{
    public record UploadAvatarCommand(Guid UserId, IFormFile File) : IRequest<Result<string>>, IRequireSameUser
    {
        public Guid TargetUserId => UserId;
    }
}
