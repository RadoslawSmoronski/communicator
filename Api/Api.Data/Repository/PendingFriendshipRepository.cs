using Api.Data.IRepository;
using Api.Models;
using Api.Models.Friendship;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.Repository
{
    public class PendingFriendshipRepository : IPendingFriendshipRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public PendingFriendshipRepository(ApplicationDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendInviteAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId))
            {
                throw new Exception("senderId is empty.");
            }

            if (string.IsNullOrWhiteSpace(recipientId))
            {
                throw new Exception("recipientId is empty.");
            }

            var user1 = await _userManager.FindByIdAsync(senderId);

            if (user1 == null)
            {
                throw new Exception("User with senderId doesn't exist.");
            }

            var user2 = await _userManager.FindByIdAsync(recipientId);

            if (user2 == null)
            {
                throw new Exception("User with recipientId doesn't exist.");
            }

            var pendingFriendship = new PendingFriendship
            {
                User1Id = senderId,
                User2Id = recipientId,
                User1 = user1,
                User2 = user2
            };

            await _context.PendingFriendships.AddAsync(pendingFriendship);
            await _context.SaveChangesAsync();
        }

        public Task DecelineInviteAsync(string recipientId, string senderId)
        {
            throw new NotImplementedException();
        }

        public Task AcceptInviteAsync(string recipientId, string senderId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PendingFriendship>> CheckInvitiesAsync(string recipientId)
        {
            throw new NotImplementedException();
        }

    }
}
