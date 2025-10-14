using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Friendships.Commands.DeleteFriendship
{
    public class DeleteFriendshipHandler : IRequestHandler<DeleteFriendshipCommand, Result>
    {
        private readonly IFriendshipService _friendshipService;

        public DeleteFriendshipHandler(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        public async Task<Result> Handle(DeleteFriendshipCommand request, CancellationToken cancellationToken)
             => await _friendshipService.DeleteAsync(request.FriendshipId);
    }
}
