using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.ConfirmEmail
{
    public record ConfirmEmailCommand(Guid UserId, string ConfirmationToken) : IRequest<Result>, IAllowAnonymous;
}
