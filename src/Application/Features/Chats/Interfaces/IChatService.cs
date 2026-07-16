using Application.Features.Chats.DataTransferObjects.Requests;
using Application.Features.Chats.DataTransferObjects.Responses;

namespace Application.Features.Chats;

public interface IChatService
{
    Task<PrivateChatDto?> FindOrCreatePrivateChatAsync(int currentUserId, CreatePrivateChatRequest request, CancellationToken cancellationToken = default);
}
