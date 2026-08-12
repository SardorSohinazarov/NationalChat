using Application.DataTransferObjects.Pagination;
using Application.Features.Messages.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Responses;

namespace Application.Features.Messages;

public interface IMessageService
{
    Task<CursorPagedResponse<MessageDto>?> GetMessagesAsync(int currentUserId, int chatId, CursorPaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessageDto>?> SearchAsync(int currentUserId, int chatId, MessageSearchRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MessageDto>?> GetContextAsync(int currentUserId, int chatId, int messageId, CancellationToken cancellationToken = default);
    Task<MessageDto?> SendAsync(int currentUserId, int chatId, SendMessageRequest request, CancellationToken cancellationToken = default);
}
