using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.DeleteAvatar
{
    public record DeleteAvatarCommand(Guid UserId) : IRequest<Result>, IRequireSameUser
    {
        public Guid TargetUserId => UserId;
    }
}
