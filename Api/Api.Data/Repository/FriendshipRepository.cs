using Api.Data.IRepository;
using Api.Exceptions.FriendshipInvitationRepository;
using Api.Exceptions;
using Api.Models;
using Api.Models.Friendship;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Models.Dtos.Controllers.FriendsController;
using Microsoft.EntityFrameworkCore;
using Api.Exceptions.Friendship;

namespace Api.Data.Repository
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public FriendshipRepository(ApplicationDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddFriendshipAsync(string user1Id, string user2Id)
        {
            if (string.IsNullOrWhiteSpace(user1Id))
            {
                throw new EnteredDataIsNullException("user1 is empty.");
            }

            if (string.IsNullOrWhiteSpace(user2Id))
            {
                throw new EnteredDataIsNullException("user2 is empty.");
            }

            if (user1Id == user2Id)
            {
                throw new InvalidOperationException("Cannot added you as a friend.");
            }

            var user1 = await _userManager.FindByIdAsync(user1Id);

            if (user1 == null)
            {
                throw new UserNotFoundException("User with user1Id doesn't exist.");
            }

            var user2 = await _userManager.FindByIdAsync(user2Id);

            if (user2 == null)
            {
                throw new UserNotFoundException("User with user2Id doesn't exist.");
            }

            //if (await IsFriendshipExists(user1, user2))
            //{
                
            //} todo

            var friendship = new Friendship
            {
                User1Id = user1Id,
                User2Id = user2Id,
                User1 = user1,
                User2 = user2
            };

            try
            {
                await _context.Friendships.AddAsync(friendship);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseOperationException();
            }
        }

        public async Task<List<FriendDto>> GetFriendsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new Exception("userId is empty.");
            }

            var user1 = await _userManager.FindByIdAsync(userId);

            if (user1 == null)
            {
                throw new Exception("User with userId doesn't exist.");
            }

            var records = await _context.Friendships
                .Where(x => x.User1Id == userId || x.User2Id == userId)
                .Select(x => new FriendDto
                {
                    Id = x.User1Id == userId ? x.User2Id : x.User1Id,
                    Username = x.User1Id == userId ? x.User2.UserName : x.User1.UserName
                })
                .ToListAsync();

            if (records.Count < 1)
            {
                throw new FriendshipDoesNotExistException();
            }

            return records;
        }
    }
}
