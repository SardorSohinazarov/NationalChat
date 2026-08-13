using Application.DataTransferObjects.Pagination;
using Application.Features.Messages.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Responses;
using Application.Features.Messages.Factories;
using Application.Features.Messages.Mappers;
using FluentValidation;

namespace Application.Features.Messages;

public sealed class MessageService(
    IMessageRepository repository,
    IChatRealtimeNotifier realtimeNotifier,
    IValidator<SendMessageRequest> sendMessageValidator,
    IValidator<UpdateMessageRequest> updateMessageValidator,
    IValidator<MessageSearchRequest> messageSearchValidator,
    TimeProvider timeProvider) : IMessageService
{
    public async Task<CursorPagedResponse<MessageDto>?> GetMessagesAsync(
        int currentUserId,
        int chatId,
        CursorPaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        if (!await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken))
        {
            return null;
        }

        var result = await repository.GetMessagesAsync(chatId, currentUserId, pagination, cancellationToken);
        var messageIds = result.Items.Select(x => x.Id).ToArray();
        await repository.MarkAsReadAsync(chatId, currentUserId, messageIds, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        await realtimeNotifier.MessagesReadAsync(chatId, currentUserId, messageIds, await repository.GetMemberUserIdsAsync(chatId, cancellationToken), cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<MessageDto>?> SearchAsync(
        int currentUserId, int chatId, MessageSearchRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await messageSearchValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid || !await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken)) return null;
        return await repository.SearchAsync(chatId, currentUserId, request.Query.Trim(), request.Limit, cancellationToken);
    }

    public async Task<IReadOnlyList<MessageDto>?> GetContextAsync(
        int currentUserId, int chatId, int messageId, CancellationToken cancellationToken = default)
    {
        if (messageId <= 0 || !await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken)) return null;
        return await repository.GetContextAsync(chatId, currentUserId, messageId, cancellationToken);
    }

    public async Task<MessageDto?> SendAsync(
        int currentUserId,
        int chatId,
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await sendMessageValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid || !await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken))
        {
            return null;
        }

        if (request.ReplyToMessageId.HasValue && !await repository.ExistsInChatAsync(request.ReplyToMessageId.Value, chatId, cancellationToken))
        {
            return null;
        }

        var message = MessageFactory.Create(chatId, currentUserId, request.TextContent, request.ReplyToMessageId, timeProvider.GetUtcNow().UtcDateTime);
        await repository.AddAsync(message, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        var messageDto = MessageMapper.ToDto((await repository.GetByIdAsync(message.Id, cancellationToken))!);
        var memberUserIds = await repository.GetMemberUserIdsAsync(chatId, cancellationToken);
        await realtimeNotifier.MessageCreatedAsync(messageDto, memberUserIds, cancellationToken);
        return messageDto;
    }

    public async Task<MessageDto?> UpdateAsync(int currentUserId, int chatId, int messageId, UpdateMessageRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await updateMessageValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return null;

        var message = await repository.GetOwnedMessageAsync(chatId, messageId, currentUserId, cancellationToken);
        if (message is null) return null;
        message.TextContent = request.TextContent.Trim();
        message.EditedAt = timeProvider.GetUtcNow().UtcDateTime;
        await repository.SaveChangesAsync(cancellationToken);
        var dto = (await repository.GetDtoAsync(message.Id, currentUserId, cancellationToken))!;
        await realtimeNotifier.MessageUpdatedAsync(dto, await repository.GetMemberUserIdsAsync(chatId, cancellationToken), cancellationToken);
        return dto;
    }

    public async Task<bool> DeleteAsync(int currentUserId, int chatId, int messageId, CancellationToken cancellationToken = default)
    {
        var message = await repository.GetOwnedMessageAsync(chatId, messageId, currentUserId, cancellationToken);
        if (message is null) return false;
        var memberIds = await repository.GetMemberUserIdsAsync(chatId, cancellationToken);
        await repository.SoftDeleteAsync(message, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        await realtimeNotifier.MessageDeletedAsync(chatId, messageId, memberIds, cancellationToken);
        return true;
    }

    public async Task<bool> ClearChatAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default)
    {
        if (!await repository.IsChatMemberAsync(chatId, currentUserId, cancellationToken)) return false;
        var memberIds = await repository.GetMemberUserIdsAsync(chatId, cancellationToken);
        await repository.ClearChatAsync(chatId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        await realtimeNotifier.ChatClearedAsync(chatId, memberIds, cancellationToken);
        return true;
    }
}
