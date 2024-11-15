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
using Api.Models.Dtos.Controllers.FriendsController;

namespace Api.Data.Repository
{
    public class FriendsRepository : IFriendsRepository
    {
        public readonly ApplicationDbContext _context;
        public FriendsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFriendsInvitationExists(string userId1, string userId2)
        {
            return await _context.FriendshipInvitations
                        .AnyAsync(x => (x.SenderId == userId1 && x.RecipientId == userId2)
                        || (x.SenderId == userId2 && x.RecipientId == userId1));
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

        public async Task<List<GetInvitationsUserDto>> GetInvitationsAsync(string userId)
        {   
            return await _context.FriendshipInvitations
                          .Where(x => x.RecipientId == userId)
                          .Select(x => new GetInvitationsUserDto
                          {
                              Id = x.SenderId,
                              UserName = x.SenderUser.UserName!
                          })
                          .ToListAsync();
        }

        public async Task<Result> DeleteInviteAsync(UserAccount senderUser, UserAccount recipientUser)
        {

            var invitation = await _context.FriendshipInvitations
                .Where(x => x.SenderId == senderUser.Id && x.RecipientId == recipientUser.Id)
                .FirstOrDefaultAsync();

            if(invitation != null)
            {
                _context.FriendshipInvitations.Remove(invitation);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            else
            {
                return Error.NotFound("INVITATION_NOT_FOUND", "Invitation not found.");
            }
        }
    }
}
