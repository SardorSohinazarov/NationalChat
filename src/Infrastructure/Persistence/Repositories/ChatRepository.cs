using Application.Features.Chats;
using Application.DataTransferObjects.Pagination;
using Application.Features.Chats.DataTransferObjects.Responses;
using Application.Features.Chats.Mappers;
using Domain.Entities;
using Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ChatRepository(ChatDb db) : IChatRepository
{
    public Task<User?> FindUserAsync(int userId, CancellationToken cancellationToken = default) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task<Chat?> FindPrivateChatAsync(int firstUserId, int secondUserId, CancellationToken cancellationToken = default) =>
        db.Chats.AsNoTracking()
            .Where(x => x.Type == ChatType.Private && x.Members.Count == 2 &&
                x.Members.Any(member => member.UserId == firstUserId) &&
                x.Members.Any(member => member.UserId == secondUserId))
            .Include(x => x.Members)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<CursorPagedResponse<ChatListDto>> GetChatsAsync(int userId, CursorPaginationRequest pagination, CancellationToken cancellationToken = default) =>
        db.Chats.AsNoTracking()
            .Where(chat => chat.Members.Any(member => member.UserId == userId))
            .ToCursorPagedResponseAsync(pagination, chat => chat.Id, ChatListMapper.Projection(userId), chat => chat.Id, cancellationToken);

    public Task AddAsync(Chat chat, CancellationToken cancellationToken = default) =>
        db.Chats.AddAsync(chat, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
