using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.RequestPasswordReset
{
    public record RequestPasswordResetCommand(string Email) : IRequest<Result<string>>, IAllowAnonymous;
}
