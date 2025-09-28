using MediatR;
using Shared.Result;

namespace Application.Users.Commands.ChangeUsername
{
    public record ChangeUsernameCommand(Guid UserId, string NewPassword) : IRequest<Result<string>>;
}
