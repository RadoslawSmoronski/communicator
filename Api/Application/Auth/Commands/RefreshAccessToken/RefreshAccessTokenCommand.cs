using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public record RefreshAccessTokenCommand(Guid RefreshToken) : IRequest<Result<RefreshAccessTokenReadModel>>, IAllowAnonymous;
}
