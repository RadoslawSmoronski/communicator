using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.ChangeUsername
{
    public record ChangeUsernameCommand(Guid UserId, string NewPassword) : IRequest<Result<string>>, IRequireSameUser
    {
        public Guid TargetUserId => UserId;
    }
}
