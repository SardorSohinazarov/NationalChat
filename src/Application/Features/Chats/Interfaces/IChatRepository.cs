using Domain.Entities;
using Application.DataTransferObjects.Pagination;
using Application.Features.Chats.DataTransferObjects.Responses;

namespace Application.Features.Chats;

public interface IChatRepository
{
    Task<User?> FindUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Chat> FindOrCreatePrivateChatAsync(int firstUserId, int secondUserId, DateTime createdAt, CancellationToken cancellationToken = default);
    Task<CursorPagedResponse<ChatListDto>> GetChatsAsync(int userId, CursorPaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>?> SoftDeleteAsync(int chatId, int userId, DateTime deletedAt, CancellationToken cancellationToken = default);
}
