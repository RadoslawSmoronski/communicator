using ChatCommunicator.Application.Services.Interfaces;
using System.Collections.Concurrent;

namespace ChatCommunicator.Application.Managers
{
    public class UsersConnectionService : IUsersConnectionService
    {
        private readonly ConcurrentDictionary<Guid, HashSet<string>> _usersOnline = new();


        public Task AddUpdateAsync(string connectionId, Guid userId)
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

        public Task RemoveAsync(string connectionId, Guid userId)
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

        public List<Guid> GetOnlineUsersIdAsync()
        {
            return _usersOnline.Keys.ToList();
        }

        public Task<bool> IsUserOnlineAsync(Guid userId)
        {
            return Task.FromResult(_usersOnline.ContainsKey(userId));
        }

        public List<string>? GetUserConnectionsId(Guid UserId)
        {
            if (_usersOnline.TryGetValue(UserId, out var connections))
            {
                return connections.ToList();
            }

            return null;
        }
    }
}
