using Api.Data.IRepository;
using Api.Models.Friendship;
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

        public PendingFriendshipRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AcceptInviteAsync(string recipientId, string senderId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PendingFriendship>> CheckInvitiesAsync(string recipientId)
        {
            throw new NotImplementedException();
        }

        public Task DecelineInviteAsync(string recipientId, string senderId)
        {
            throw new NotImplementedException();
        }

        public Task SendInviteAsync(string senderId, string recipientId)
        {
            throw new NotImplementedException();
        }
    }
}
