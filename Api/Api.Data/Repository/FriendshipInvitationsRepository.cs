﻿using Api.Data.IRepository;
using Api.Exceptions;
using Api.Exceptions.FriendshipInvitationRepository;
using Api.Models;   
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.Repository
{
    public class FriendshipInvitationsRepository : IFriendshipInvitationsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public FriendshipInvitationsRepository(ApplicationDbContext context, UserManager<UserAccount> userManager)
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

            var senderUser = await _userManager.FindByIdAsync(senderId);

            if (senderUser == null)
            {
                throw new UserNotFoundException("User with senderId doesn't exist.");
            }

            var recipientUser = await _userManager.FindByIdAsync(recipientId);

            if (recipientUser == null)
            {
                throw new UserNotFoundException("User with recipientId doesn't exist.");
            }

            if(await IsFriendshipInvitationExists(senderId, recipientId))
            {
                throw new FriendshipInvitationIsAlreadyExistException();
            }

            var friendshipInvitation = new FriendshipInvitation
            {
                SenderId = senderId,
                RecipientId = recipientId,
                SenderUser = senderUser,
                RecipientUser = recipientUser
            };

            try
            {
                await _context.FriendshipInvitations.AddAsync(friendshipInvitation);
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

        public async Task<List<FriendsInviteUserDto>> GetUserInvitiesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new EnteredDataIsNullException("userId is empty.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new UserNotFoundException("This user doesn't exist.");
            }

            //var records = await _context.PendingFriendships
            //    .Where(x => (x.User1Id == userId || x.User2Id == userId))
            //    .ToListAsync();

            //dorobic driendsinviteuserdto na format odpowiedni i zwrocic z tym liste zamiasrt pendingfriends

            //if (records != null && records.Count > 0)
            //{
            //    return records;
            //}

            throw new FriendshipInvitationDoesNotExistException();
        }

        public async Task<bool> IsFriendshipInvitationExists(string userId1, string userId2)
        {
            throw new NotImplementedException();
            //if (string.IsNullOrWhiteSpace(userId1))
            //{
            //    throw new Exception("username1 is empty.");
            //}

            //if (string.IsNullOrWhiteSpace(userId2))
            //{
            //    throw new Exception("username2 is empty.");
            //}

            //var user1 = await _userManager.FindByIdAsync(userId1);

            //if (user1 == null)
            //{
            //    throw new Exception("User with senderId doesn't exist.");
            //}

            //var user2 = await _userManager.FindByIdAsync(userId2);

            //if (user2 == null)
            //{
            //    throw new Exception("User with recipientId doesn't exist.");
            //}

            //var exists = await _context.FriendshipInvitations
            //    .AnyAsync(x => (x.SenderId == userId1 && x.RecipientId == userId2)
            //                 || (x.SenderId == userId2 && x.RecipientId == userId1));

            //return exists;
        }
    }
}
