using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(Guid UserId, string CodedToken, string NewPassword) : IRequest<Result<string>>;
}
