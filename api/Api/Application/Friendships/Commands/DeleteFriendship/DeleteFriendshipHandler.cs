using Application.Common.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Friendships.Commands.DeleteFriendship
{
    public class DeleteFriendshipHandler(IFriendshipService friendshipService) : IRequestHandler<DeleteFriendshipCommand, Result>
    {
        private readonly IFriendshipService _friendshipService = friendshipService;

        public async Task<Result> Handle(DeleteFriendshipCommand request, CancellationToken cancellationToken)
             => await _friendshipService.DeleteAsync(request.FriendshipId);
    }
}
