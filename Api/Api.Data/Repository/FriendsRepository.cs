using Api.Data.IRepository;
using Api.Exceptions.FriendshipInvitationRepository;
using Api.Exceptions;
using Api.Models.Friendship;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using Api.Utilities.Result;

namespace Api.Data.Repository
{
    public class FriendsRepository : IFriendsRepository
    {
        public readonly ApplicationDbContext _context;
        public FriendsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendInviteAsync(UserAccount senderUser, UserAccount recipientUser)
        {
            var friendshipInvitation = new FriendshipInvitation
            {
                SenderId = senderUser.Id,
                RecipientId = recipientUser.Id,
                SenderUser = senderUser,
                RecipientUser = recipientUser
            };

            await _context.FriendshipInvitations.AddAsync(friendshipInvitation);
            await _context.SaveChangesAsync();
        }
    }
}
