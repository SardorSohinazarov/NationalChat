namespace Application.Features.Presence;

public interface IPresenceTracker
{
    bool IsOnline(int userId);

    /// <returns>true if this connection made the user transition from offline to online.</returns>
    bool AddConnection(int userId, string connectionId);

    /// <returns>true if this connection made the user transition from online to offline.</returns>
    bool RemoveConnection(int userId, string connectionId);
}
