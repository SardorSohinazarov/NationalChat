using Application.Features.Presence;
using StackExchange.Redis;

namespace Infrastructure.Realtime;

public sealed class RedisPresenceTracker(IConnectionMultiplexer redis) : IPresenceTracker
{
    private readonly IDatabase db = redis.GetDatabase();

    private static string Key(int userId) => $"presence:user:{userId}";

    public bool IsOnline(int userId) => db.SetLength(Key(userId)) > 0;

    public bool AddConnection(int userId, string connectionId)
    {
        var key = Key(userId);
        db.SetAdd(key, connectionId);
        return db.SetLength(key) == 1;
    }

    public bool RemoveConnection(int userId, string connectionId)
    {
        var key = Key(userId);
        db.SetRemove(key, connectionId);
        return db.SetLength(key) == 0;
    }
}
