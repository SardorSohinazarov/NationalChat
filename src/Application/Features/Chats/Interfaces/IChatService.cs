using Application.Features.Chats.DataTransferObjects.Requests;
using Application.Features.Chats.DataTransferObjects.Responses;
using Application.DataTransferObjects.Pagination;

namespace Application.Features.Chats;

public interface IChatService
{
    Task<CursorPagedResponse<ChatListDto>> GetChatsAsync(int currentUserId, CursorPaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<PrivateChatDto?> FindOrCreatePrivateChatAsync(int currentUserId, CreatePrivateChatRequest request, CancellationToken cancellationToken = default);
}
