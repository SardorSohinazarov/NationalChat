using System.Collections.Concurrent;
using Application.Features.Presence;

namespace Infrastructure.Realtime;

public sealed class InMemoryPresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, byte>> connectionsByUser = new();

    public bool IsOnline(int userId) =>
        connectionsByUser.TryGetValue(userId, out var connections) && !connections.IsEmpty;

    public bool AddConnection(int userId, string connectionId)
    {
        var connections = connectionsByUser.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        connections.TryAdd(connectionId, 0);
        return connections.Count == 1;
    }

    public bool RemoveConnection(int userId, string connectionId)
    {
        if (!connectionsByUser.TryGetValue(userId, out var connections))
        {
            return false;
        }

        connections.TryRemove(connectionId, out _);
        if (!connections.IsEmpty)
        {
            return false;
        }

        connectionsByUser.TryRemove(userId, out _);
        return true;
    }
}
