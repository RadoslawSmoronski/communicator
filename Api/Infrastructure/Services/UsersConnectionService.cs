using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class UsersConnectionService(ILogger<UsersConnectionService> logger) : IUsersConnectionService
    {
        private readonly ConcurrentDictionary<Guid, HashSet<string>> _usersOnline = new();
        private readonly ILogger<UsersConnectionService> _logger = logger;

        public Task AddUpdateAsync(string connectionId, Guid userId)
        {
            _logger.LogInformation("Adding/updating connection. UserId: {UserId}, ConnectionId: {ConnectionId}", userId, connectionId);

            _usersOnline.AddOrUpdate(userId,
                _ =>
                {
                    _logger.LogDebug("Creating new connection set for UserId: {UserId}", userId);
                    return new HashSet<string> { connectionId };
                },
                (_, existing) =>
                {
                    lock (existing)
                    {
                        existing.Add(connectionId);
                        _logger.LogDebug("Added connectionId to existing set for UserId: {UserId}. Connections count: {Count}", userId, existing.Count);
                    }
                    return existing;
                });

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string connectionId, Guid userId)
        {
            _logger.LogInformation("Removing connection. UserId: {UserId}, ConnectionId: {ConnectionId}", userId, connectionId);

            if (_usersOnline.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    bool removed = connections.Remove(connectionId);
                    if (removed)
                    {
                        _logger.LogDebug("Removed connectionId from UserId: {UserId}. Remaining connections: {Count}", userId, connections.Count);
                    }
                    else
                    {
                        _logger.LogWarning("Attempted to remove non-existing connectionId for UserId: {UserId}", userId);
                    }

                    if (connections.Count == 0)
                    {
                        bool removedUser = _usersOnline.TryRemove(userId, out _);
                        if (removedUser)
                        {
                            _logger.LogInformation("No more connections for UserId: {UserId}. User removed from online list.", userId);
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("Attempted to remove connection for UserId: {UserId} which was not found online.", userId);
            }

            return Task.CompletedTask;
        }

        public async Task<List<Guid>> GetOnlineUsersIdAsync()
        {
            _logger.LogDebug("Getting list of online users. Count: {Count}", _usersOnline.Count);
            return await Task.FromResult(_usersOnline.Keys.ToList());
        }

        public Task<bool> IsUserOnlineAsync(Guid userId)
        {
            bool isOnline = _usersOnline.ContainsKey(userId);
            _logger.LogDebug("IsUserOnlineAsync check for UserId: {UserId} - Online: {IsOnline}", userId, isOnline);
            return Task.FromResult(isOnline);
        }

        public List<string>? GetUserConnectionsId(Guid userId)
        {
            if (_usersOnline.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    _logger.LogDebug("GetUserConnectionsId: UserId {UserId} has {Count} connections", userId, connections.Count);
                    return connections.ToList();
                }
            }

            _logger.LogDebug("GetUserConnectionsId: UserId {UserId} not found online", userId);
            return null;
        }

    }
}
