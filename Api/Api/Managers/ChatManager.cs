using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Identity;

namespace Api.Managers
{
    public class ChatManager : IChatManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IFriendsManager _friendsManager;

        public ChatManager(IUnitOfWork unitOfWork, UserManager<UserAccount> userManager, IFriendsManager friendsManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _friendsManager = friendsManager;   
        }

        public async Task<ResultT<Conversation>> GetOrCreateConversationAsync(string userId, string friendId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(friendId))
            {
                return Error.BadRequest("USERID_IS_EMPTY", "UserId or FriendId cannot be null or empty.");
            }

            if (userId == friendId)
            {
                return Error.BadRequest("USERID_AND_FRIENDID_ARE_THE_SAME", "UserId and FriendId must be different.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USER_NOT_FOUND", "User was not found.");
                }

                var friendUser = await _userManager.FindByIdAsync(friendId);

                if (friendUser == null || friendUser.UserName == null)
                {
                    return Error.NotFound("FRIENDUSER_NOT_FOUND", "Friend was not found.");
                }

                var conversation = await GetConversationAsync(userId, friendId);

                if (conversation == null)
                {
                    conversation = await CreateConversationAsync(user, friendUser);
                }

                return conversation;
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<Conversation?> GetConversationAsync(string user1Id, string user2Id)
        {
            return await _unitOfWork.Conversations.FirstOrDefaultAsync(x =>
                (x.User1Id == user1Id || x.User2Id == user2Id) ||
                (x.User1Id == user2Id || x.User2Id == user1Id));
        }

        private async Task<Conversation> CreateConversationAsync(UserAccount user1, UserAccount user2)
        {
            var conversation = new Conversation()
            {
                User1Id = user1.Id,
                User2Id = user2.Id,
                User1 = user1,
                User2 = user2
            };

            await _unitOfWork.Conversations.AddAsync(conversation);
            await _unitOfWork.SaveAsync();

            return conversation;
        }
    }
}
