using Api.Data.Repository;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Api.Managers
{
    public class UsersConnectionManager : IUsersConnectionManager
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> _usersOnline = new();


        public Task AddUpdateAsync(string connectionId, string userId)
        {
            _usersOnline.AddOrUpdate(userId,
                _ => new HashSet<string> { connectionId },
                (_, existing) =>
                {
                    lock (existing)
                    {
                        existing.Add(connectionId);
                    }
                    return existing;
                });

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string connectionId, string userId)
        {
            if (_usersOnline.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _usersOnline.TryRemove(userId, out _);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public async Task<List<string>> GetOnlineUsersIdAsync()
        {
            return _usersOnline.Keys.ToList();
        }

        public Task<bool> IsUserOnlineAsync(string userId)
        {
            return Task.FromResult(_usersOnline.ContainsKey(userId));
        }

        public async Task<List<string>> GetUserConnectionsId(string UserId)
        {
            return _usersOnline[UserId].ToList();
        }
    }
}
