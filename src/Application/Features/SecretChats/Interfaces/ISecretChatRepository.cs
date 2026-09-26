using Domain.Entities;

namespace Application.Features.SecretChats;

public interface ISecretChatRepository
{
    /// <summary>Access tokens outlive a logout, so every secret-chat call re-checks that the device session is still valid.</summary>
    Task<bool> IsSessionActiveAsync(int userId, int sessionId, DateTime now, CancellationToken cancellationToken = default);

    /// <summary>A user with a completed profile, or null.</summary>
    Task<User?> FindUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Tracked chat with both users loaded.</summary>
    Task<SecretChat?> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Open chats bound to this device, plus pending requests addressed to the user (visible on all of their devices).</summary>
    Task<IReadOnlyList<SecretChat>> GetVisibleAsync(int userId, int sessionId, CancellationToken cancellationToken = default);

    Task<bool> HasPendingRequestAsync(int initiatorSessionId, int participantId, CancellationToken cancellationToken = default);

    Task AddAsync(SecretChat chat, CancellationToken cancellationToken = default);

    /// <summary>Saves the message together with pending chat changes; false when the (chat, session, seq) triple already exists.</summary>
    Task<bool> TryAddMessageAsync(SecretMessage message, CancellationToken cancellationToken = default);

    /// <summary>Messages addressed to <paramref name="recipientSessionId"/> (i.e. sent by the other device), oldest first.</summary>
    Task<IReadOnlyList<SecretMessage>> GetQueueAsync(int secretChatId, int recipientSessionId, long? afterId, int take, CancellationToken cancellationToken = default);

    /// <summary>Removes messages addressed to <paramref name="recipientSessionId"/> with ids up to <paramref name="upToId"/>.</summary>
    Task DeleteDeliveredAsync(int secretChatId, int recipientSessionId, long upToId, CancellationToken cancellationToken = default);

    Task DeleteMessagesAsync(int secretChatId, CancellationToken cancellationToken = default);

    /// <summary>Tracked open (pending or active) chats bound to any of the sessions, with both users loaded.</summary>
    Task<IReadOnlyList<SecretChat>> GetOpenBySessionsAsync(IReadOnlyCollection<int> sessionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracked open chats that must be closed: a bound session was revoked or expired, or the request has been
    /// pending since before <paramref name="pendingCreatedBefore"/>.
    /// </summary>
    Task<IReadOnlyList<SecretChat>> GetStaleAsync(DateTime now, DateTime pendingCreatedBefore, int take, CancellationToken cancellationToken = default);

    /// <summary>Drops undelivered ciphertext older than <paramref name="createdBefore"/>; returns the number removed.</summary>
    Task<int> DeleteUndeliveredBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default);

    Task AddFileAsync(SecretFile file, CancellationToken cancellationToken = default);

    Task<SecretFile?> GetFileAsync(int secretChatId, Guid fileId, CancellationToken cancellationToken = default);

    Task<int> CountFilesAsync(int secretChatId, CancellationToken cancellationToken = default);

    void RemoveFile(SecretFile file);

    /// <summary>Deletes the chat's file records and returns their ids so the blobs can be removed too.</summary>
    Task<IReadOnlyList<Guid>> DeleteFilesOfChatAsync(int secretChatId, CancellationToken cancellationToken = default);

    /// <summary>Deletes file records older than <paramref name="createdBefore"/> and returns their ids.</summary>
    Task<IReadOnlyList<Guid>> DeleteFilesCreatedBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
