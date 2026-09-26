using Application.Features.SecretChats;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Persistence.Repositories;

public sealed class SecretChatRepository(ChatDb db) : ISecretChatRepository
{
    public Task<bool> IsSessionActiveAsync(int userId, int sessionId, DateTime now, CancellationToken cancellationToken = default) =>
        db.Sessions.AnyAsync(session => session.Id == sessionId && session.UserId == userId &&
            session.RevokedAt == null && session.ExpiresAt > now, cancellationToken);

    public Task<User?> FindUserAsync(int userId, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(user => user.Id == userId && user.IsProfileCompleted, cancellationToken);

    public Task<SecretChat?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        WithUsers().FirstOrDefaultAsync(chat => chat.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SecretChat>> GetVisibleAsync(int userId, int sessionId, CancellationToken cancellationToken = default) =>
        await WithUsers().AsNoTracking()
            .Where(chat => chat.Status != SecretChatStatus.Closed && (
                (chat.InitiatorId == userId && chat.InitiatorSessionId == sessionId) ||
                (chat.ParticipantId == userId && (chat.ParticipantSessionId == sessionId ||
                    (chat.ParticipantSessionId == null && chat.Status == SecretChatStatus.Pending)))))
            .OrderByDescending(chat => chat.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> HasPendingRequestAsync(int initiatorSessionId, int participantId, CancellationToken cancellationToken = default) =>
        db.SecretChats.AnyAsync(chat => chat.InitiatorSessionId == initiatorSessionId &&
            chat.ParticipantId == participantId && chat.Status == SecretChatStatus.Pending, cancellationToken);

    public Task AddAsync(SecretChat chat, CancellationToken cancellationToken = default) =>
        db.SecretChats.AddAsync(chat, cancellationToken).AsTask();

    public async Task<bool> TryAddMessageAsync(SecretMessage message, CancellationToken cancellationToken = default)
    {
        var entry = await db.SecretMessages.AddAsync(message, cancellationToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    public async Task<IReadOnlyList<SecretMessage>> GetQueueAsync(int secretChatId, int recipientSessionId, long? afterId, int take, CancellationToken cancellationToken = default) =>
        await db.SecretMessages.AsNoTracking()
            .Where(message => message.SecretChatId == secretChatId && message.SenderSessionId != recipientSessionId &&
                (afterId == null || message.Id > afterId))
            .OrderBy(message => message.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task DeleteDeliveredAsync(int secretChatId, int recipientSessionId, long upToId, CancellationToken cancellationToken = default) =>
        db.SecretMessages
            .Where(message => message.SecretChatId == secretChatId && message.SenderSessionId != recipientSessionId && message.Id <= upToId)
            .ExecuteDeleteAsync(cancellationToken);

    public Task DeleteMessagesAsync(int secretChatId, CancellationToken cancellationToken = default) =>
        db.SecretMessages.Where(message => message.SecretChatId == secretChatId).ExecuteDeleteAsync(cancellationToken);

    public async Task<IReadOnlyList<SecretChat>> GetOpenBySessionsAsync(IReadOnlyCollection<int> sessionIds, CancellationToken cancellationToken = default) =>
        await WithUsers()
            .Where(chat => chat.Status != SecretChatStatus.Closed && (
                sessionIds.Contains(chat.InitiatorSessionId) ||
                (chat.ParticipantSessionId != null && sessionIds.Contains(chat.ParticipantSessionId.Value))))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SecretChat>> GetStaleAsync(DateTime now, DateTime pendingCreatedBefore, int take, CancellationToken cancellationToken = default) =>
        await WithUsers()
            .Where(chat => chat.Status != SecretChatStatus.Closed && (
                (chat.Status == SecretChatStatus.Pending && chat.CreatedAt < pendingCreatedBefore) ||
                chat.InitiatorSession.RevokedAt != null || chat.InitiatorSession.ExpiresAt <= now ||
                (chat.ParticipantSession != null && (chat.ParticipantSession.RevokedAt != null || chat.ParticipantSession.ExpiresAt <= now))))
            .OrderBy(chat => chat.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> DeleteUndeliveredBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default) =>
        db.SecretMessages.Where(message => message.CreatedAt < createdBefore).ExecuteDeleteAsync(cancellationToken);

    public Task AddFileAsync(SecretFile file, CancellationToken cancellationToken = default) =>
        db.SecretFiles.AddAsync(file, cancellationToken).AsTask();

    public Task<SecretFile?> GetFileAsync(int secretChatId, Guid fileId, CancellationToken cancellationToken = default) =>
        db.SecretFiles.FirstOrDefaultAsync(file => file.Id == fileId && file.SecretChatId == secretChatId, cancellationToken);

    public Task<int> CountFilesAsync(int secretChatId, CancellationToken cancellationToken = default) =>
        db.SecretFiles.CountAsync(file => file.SecretChatId == secretChatId, cancellationToken);

    public void RemoveFile(SecretFile file) => db.SecretFiles.Remove(file);

    public Task<IReadOnlyList<Guid>> DeleteFilesOfChatAsync(int secretChatId, CancellationToken cancellationToken = default) =>
        DeleteFilesAsync(db.SecretFiles.Where(file => file.SecretChatId == secretChatId), cancellationToken);

    public Task<IReadOnlyList<Guid>> DeleteFilesCreatedBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default) =>
        DeleteFilesAsync(db.SecretFiles.Where(file => file.CreatedAt < createdBefore), cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);

    private static async Task<IReadOnlyList<Guid>> DeleteFilesAsync(IQueryable<SecretFile> files, CancellationToken cancellationToken)
    {
        var ids = await files.Select(file => file.Id).ToListAsync(cancellationToken);
        if (ids.Count > 0) await files.Where(file => ids.Contains(file.Id)).ExecuteDeleteAsync(cancellationToken);
        return ids;
    }

    private IQueryable<SecretChat> WithUsers() =>
        db.SecretChats.Include(chat => chat.Initiator).Include(chat => chat.Participant);
}
