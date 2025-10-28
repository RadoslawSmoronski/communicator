using Application.Common.Security;
using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public record RefreshAccessTokenCommand(Guid RefreshToken) : IRequest<Result<RefreshAccessTokenResponseDto>>, IAllowAnonymous;
}
