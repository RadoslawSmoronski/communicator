using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginUserReadModel>>, IAllowAnonymous;
}
