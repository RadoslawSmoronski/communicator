using MediatR;
using Shared.Result;

namespace Application.Users.Commands.ChangePassword
{
    public record ChangePasswordCommand(Guid UserId, string OldPassword, string NewPassword) : IRequest<Result>;
}
