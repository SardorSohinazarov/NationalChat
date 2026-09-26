using Application.Features.SecretChats;
using Domain.Entities;

namespace NationalChat.Tests.Support;

/// <summary>
/// In-memory <see cref="ISecretChatRepository"/>: assigns ids on save and enforces the unique
/// (chat, sender session, seq) index like PostgreSQL does.
/// </summary>
public sealed class FakeSecretChatRepository : ISecretChatRepository
{
    private int _nextChatId = 100;
    private long _nextMessageId = 1;

    public List<User> Users { get; } = [];
    public List<Session> Sessions { get; } = [];
    public List<SecretChat> Chats { get; } = [];
    public List<SecretMessage> Messages { get; } = [];
    public List<SecretFile> Files { get; } = [];

    public Task<bool> IsSessionActiveAsync(int userId, int sessionId, DateTime now, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.Any(s => s.Id == sessionId && s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > now));

    public Task<User?> FindUserAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == userId && u.IsProfileCompleted));

    public Task<SecretChat?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Chats.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<SecretChat>> GetVisibleAsync(int userId, int sessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SecretChat>>(Chats
            .Where(c => c.Status != SecretChatStatus.Closed && (
                (c.InitiatorId == userId && c.InitiatorSessionId == sessionId) ||
                (c.ParticipantId == userId && (c.ParticipantSessionId == sessionId ||
                    (c.ParticipantSessionId == null && c.Status == SecretChatStatus.Pending)))))
            .OrderByDescending(c => c.Id)
            .ToList());

    public Task<bool> HasPendingRequestAsync(int initiatorSessionId, int participantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Chats.Any(c => c.InitiatorSessionId == initiatorSessionId && c.ParticipantId == participantId && c.Status == SecretChatStatus.Pending));

    public Task AddAsync(SecretChat chat, CancellationToken cancellationToken = default)
    {
        Chats.Add(chat);
        return Task.CompletedTask;
    }

    public Task<bool> TryAddMessageAsync(SecretMessage message, CancellationToken cancellationToken = default)
    {
        if (Messages.Any(m => m.SecretChatId == message.SecretChatId && m.SenderSessionId == message.SenderSessionId && m.Seq == message.Seq))
        {
            return Task.FromResult(false);
        }

        message.Id = _nextMessageId++;
        Messages.Add(message);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<SecretMessage>> GetQueueAsync(int secretChatId, int recipientSessionId, long? afterId, int take, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SecretMessage>>(Messages
            .Where(m => m.SecretChatId == secretChatId && m.SenderSessionId != recipientSessionId && (afterId == null || m.Id > afterId))
            .OrderBy(m => m.Id)
            .Take(take)
            .ToList());

    public Task DeleteDeliveredAsync(int secretChatId, int recipientSessionId, long upToId, CancellationToken cancellationToken = default)
    {
        Messages.RemoveAll(m => m.SecretChatId == secretChatId && m.SenderSessionId != recipientSessionId && m.Id <= upToId);
        return Task.CompletedTask;
    }

    public Task DeleteMessagesAsync(int secretChatId, CancellationToken cancellationToken = default)
    {
        Messages.RemoveAll(m => m.SecretChatId == secretChatId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SecretChat>> GetOpenBySessionsAsync(IReadOnlyCollection<int> sessionIds, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SecretChat>>(Chats
            .Where(c => c.Status != SecretChatStatus.Closed && (sessionIds.Contains(c.InitiatorSessionId) ||
                (c.ParticipantSessionId is { } participantSessionId && sessionIds.Contains(participantSessionId))))
            .ToList());

    public Task<IReadOnlyList<SecretChat>> GetStaleAsync(DateTime now, DateTime pendingCreatedBefore, int take, CancellationToken cancellationToken = default)
    {
        bool Inactive(int? sessionId) => sessionId is { } id && Sessions.First(s => s.Id == id) is var s && (s.RevokedAt != null || s.ExpiresAt <= now);
        return Task.FromResult<IReadOnlyList<SecretChat>>(Chats
            .Where(c => c.Status != SecretChatStatus.Closed && (
                (c.Status == SecretChatStatus.Pending && c.CreatedAt < pendingCreatedBefore) ||
                Inactive(c.InitiatorSessionId) || Inactive(c.ParticipantSessionId)))
            .OrderBy(c => c.Id)
            .Take(take)
            .ToList());
    }

    public Task<int> DeleteUndeliveredBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default) =>
        Task.FromResult(Messages.RemoveAll(m => m.CreatedAt < createdBefore));

    public Task AddFileAsync(SecretFile file, CancellationToken cancellationToken = default)
    {
        Files.Add(file);
        return Task.CompletedTask;
    }

    public Task<SecretFile?> GetFileAsync(int secretChatId, Guid fileId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Files.FirstOrDefault(f => f.Id == fileId && f.SecretChatId == secretChatId));

    public Task<int> CountFilesAsync(int secretChatId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Files.Count(f => f.SecretChatId == secretChatId));

    public void RemoveFile(SecretFile file) => Files.Remove(file);

    public Task<IReadOnlyList<Guid>> DeleteFilesOfChatAsync(int secretChatId, CancellationToken cancellationToken = default) =>
        Task.FromResult(RemoveFiles(f => f.SecretChatId == secretChatId));

    public Task<IReadOnlyList<Guid>> DeleteFilesCreatedBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default) =>
        Task.FromResult(RemoveFiles(f => f.CreatedAt < createdBefore));

    private IReadOnlyList<Guid> RemoveFiles(Func<SecretFile, bool> match)
    {
        var removed = Files.Where(match).Select(f => f.Id).ToList();
        Files.RemoveAll(f => removed.Contains(f.Id));
        return removed;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var chat in Chats.Where(c => c.Id == 0)) chat.Id = _nextChatId++;
        return Task.CompletedTask;
    }
}
