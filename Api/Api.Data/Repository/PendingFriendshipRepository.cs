using Api.Data.IRepository;
using Api.Exceptions;
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
                throw new EnteredDataIsNullException("senderId is empty.");
            }

            if (string.IsNullOrWhiteSpace(recipientId))
            {
                throw new EnteredDataIsNullException("recipientId is empty.");
            }

            if (senderId == recipientId)
            {
                throw new InvalidOperationException("Cannot send an invite to yourself.");
            }

            var user1 = await _userManager.FindByIdAsync(senderId);

            if (user1 == null)
            {
                throw new UserNotFoundException("User with senderId doesn't exist.");
            }

            var user2 = await _userManager.FindByIdAsync(recipientId);

            if (user2 == null)
            {
                throw new UserNotFoundException("User with recipientId doesn't exist.");
            }

            if(await IsFriendshipPendingExists(senderId, recipientId))
            {
                throw new Exception("This invitation already exists.");
            }

            var pendingFriendship = new PendingFriendship
            {
                User1Id = senderId,
                User2Id = recipientId,
                User1 = user1,
                User2 = user2
            };

            try
            {
                await _context.PendingFriendships.AddAsync(pendingFriendship);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseOperationException();
            }
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

        public async Task<bool> IsFriendshipPendingExists(string userId1, string userId2)
        {
            if (string.IsNullOrWhiteSpace(userId1))
            {
                throw new Exception("username1 is empty.");
            }

            if (string.IsNullOrWhiteSpace(userId2))
            {
                throw new Exception("username2 is empty.");
            }

            var user1 = await _userManager.FindByIdAsync(userId1);

            if (user1 == null)
            {
                throw new Exception("User with senderId doesn't exist.");
            }

            var user2 = await _userManager.FindByIdAsync(userId2);

            if (user2 == null)
            {
                throw new Exception("User with recipientId doesn't exist.");
            }

            var record = _context.Friendships.Where(x => (x.User1Id == userId1 && x.User2Id == userId2)
                                        || (x.User1Id == userId2 && x.User2Id == userId1));

            if(record != null)
            {
                return true;
            }

            return false;
        }
    }
}
