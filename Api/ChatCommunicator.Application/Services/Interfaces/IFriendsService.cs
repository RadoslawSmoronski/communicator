using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.FriendsController;
using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Application.Services.Interfaces
{
    public interface IFriendsService
    {
        Task<ResultT<Guid>> SendInviteAsync(Guid senderId, Guid recipientId);
        Task<ResultT<List<FriendshipInvitationDto>>> GetInvitationsAsync(Guid userId);
        Task<ResultT<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(Guid userId, string text);
        Task<Result> DecelineInviteAsync(Guid senderId, Guid recipientId);
        Task<ResultT<Guid>> AddFriendsAsync(Guid senderId, Guid recipientId);
        Task<ResultT<List<SimpleUserWithAvatarDto>>> GetFriendsAsync(Guid userId);
        Task<bool> IsFriendsExistAsync(Guid userId1, Guid userId2);
        Task<ResultT<List<string>>> GetUserOnlineFriendsConnectionsIdAsync(Guid userId);
    }
}
