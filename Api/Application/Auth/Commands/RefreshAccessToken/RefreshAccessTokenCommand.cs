using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public record RefreshAccessTokenCommand(Guid refreshToken) : IRequest<Result<RefreshAccessTokenResponseDto>>;
}
