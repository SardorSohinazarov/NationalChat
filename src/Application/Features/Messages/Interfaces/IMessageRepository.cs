using Application.DataTransferObjects.Pagination;
using Application.Features.Messages.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Messages;

public interface IMessageRepository
{
    Task<bool> IsChatMemberAsync(int chatId, int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsInChatAsync(int messageId, int chatId, CancellationToken cancellationToken = default);
    Task<CursorPagedResponse<MessageDto>> GetMessagesAsync(int chatId, int currentUserId, CursorPaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Message?> GetByIdAsync(int messageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetMemberUserIdsAsync(int chatId, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int chatId, int userId, IReadOnlyCollection<int> messageIds, DateTime viewedAt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
