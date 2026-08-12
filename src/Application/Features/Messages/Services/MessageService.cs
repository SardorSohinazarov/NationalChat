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
}
