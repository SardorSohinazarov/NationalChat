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
            .Where(chat => chat.Members.Any(member => member.UserId == userId))
            .ToCursorPagedResponseAsync(pagination, chat => chat.Id, ChatListMapper.Projection(userId), chat => chat.Id, cancellationToken);

    private Task<Chat?> FindPrivateChatAsync(int firstUserId, int secondUserId, CancellationToken cancellationToken) =>
        db.Chats
            .Where(x => x.Type == ChatType.Private && x.Members.Count == 2 &&
                x.Members.Any(member => member.UserId == firstUserId) &&
                x.Members.Any(member => member.UserId == secondUserId))
            .Include(x => x.Members)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(cancellationToken);
}
