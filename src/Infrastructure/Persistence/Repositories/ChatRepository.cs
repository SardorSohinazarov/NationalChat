using Application.Features.Chats;
using Application.DataTransferObjects.Pagination;
using Application.Features.Chats.DataTransferObjects.Responses;
using Application.Features.Chats.Mappers;
using Application.Features.Chats.Factories;
using Domain.Entities;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ChatRepository(ChatDb db) : IChatRepository
{
    public Task<User?> FindUserAsync(int userId, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public async Task<Chat> FindOrCreatePrivateChatAsync(
        int firstUserId,
        int secondUserId,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        var firstLockId = Math.Min(firstUserId, secondUserId);
        var secondLockId = Math.Max(firstUserId, secondUserId);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0}, {1})",
            [firstLockId, secondLockId],
            cancellationToken);

        var chat = await FindPrivateChatAsync(firstUserId, secondUserId, cancellationToken);
        if (chat is null)
        {
            chat = PrivateChatFactory.Create(firstUserId, secondUserId, createdAt);
            await db.Chats.AddAsync(chat, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return chat;
    }

    public Task<CursorPagedResponse<ChatListDto>> GetChatsAsync(int userId, CursorPaginationRequest pagination, CancellationToken cancellationToken = default) =>
        db.Chats.AsNoTracking()
            // A chat becomes visible only after its first message.  The latest message ID is
            // also the cursor and sort key, so an incoming message moves its chat to the top.
            .Where(chat => chat.Members.Any(member => member.UserId == userId) && chat.Messages.Any())
            .ToCursorPagedResponseAsync(
                pagination,
                chat => chat.Messages.Max(message => message.Id),
                ChatListMapper.Projection(userId),
                chat => chat.LastMessage!.Id,
                cancellationToken);

    public async Task<IReadOnlyCollection<int>?> SoftDeleteAsync(int chatId, int userId, DateTime deletedAt, CancellationToken cancellationToken = default)
    {
        var chat = await db.Chats.Include(chat => chat.Members).FirstOrDefaultAsync(chat => chat.Id == chatId && chat.Members.Any(member => member.UserId == userId), cancellationToken);
        if (chat is null) return null;
        chat.DeletedAt = deletedAt;
        await db.SaveChangesAsync(cancellationToken);
        return chat.Members.Select(member => member.UserId).ToArray();
    }

    private Task<Chat?> FindPrivateChatAsync(int firstUserId, int secondUserId, CancellationToken cancellationToken) =>
        db.Chats
            .Where(x => x.Type == ChatType.Private && x.Members.Count == 2 &&
                x.Members.Any(member => member.UserId == firstUserId) &&
                x.Members.Any(member => member.UserId == secondUserId))
            .Include(x => x.Members)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(cancellationToken);
}
