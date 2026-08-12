using Application.Features.Chats.DataTransferObjects.Requests;
using Application.Features.Chats.DataTransferObjects.Responses;
using Application.Features.Chats.Mappers;
using Application.Features.Messages;
using Application.DataTransferObjects.Pagination;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Chats;

public sealed class ChatService(
    IChatRepository repository,
    IChatRealtimeNotifier realtimeNotifier,
    IValidator<CreatePrivateChatRequest> createPrivateChatValidator,
    TimeProvider timeProvider) : IChatService
{
    public Task<CursorPagedResponse<ChatListDto>> GetChatsAsync(
        int currentUserId,
        CursorPaginationRequest pagination,
        CancellationToken cancellationToken = default) =>
        repository.GetChatsAsync(currentUserId, pagination, cancellationToken);

    public async Task<PrivateChatDto?> FindOrCreatePrivateChatAsync(
        int currentUserId,
        CreatePrivateChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await createPrivateChatValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid || request.UserId == currentUserId)
        {
            return null;
        }

        var participant = await repository.FindUserAsync(request.UserId, cancellationToken);
        if (participant is null || !participant.IsProfileCompleted)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var chat = await repository.FindOrCreatePrivateChatAsync(currentUserId, participant.Id, now, cancellationToken);
        return PrivateChatMapper.ToDto(chat, participant);
    }

    public async Task<bool> DeleteAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default)
    {
        var memberIds = await repository.SoftDeleteAsync(chatId, currentUserId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        if (memberIds is null) return false;
        await realtimeNotifier.ChatDeletedAsync(chatId, memberIds, cancellationToken);
        return true;
    }
}
