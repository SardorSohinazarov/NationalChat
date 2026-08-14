using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Responses;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public sealed class SignalRChatRealtimeNotifier(
    IHubContext<ChatHub> hubContext,
    ILogger<SignalRChatRealtimeNotifier> logger) : IChatRealtimeNotifier
{
    private Task PublishAsync(string eventName, object payload, IReadOnlyCollection<int> recipientUserIds, CancellationToken cancellationToken) =>
        hubContext.Clients.Groups(recipientUserIds.Select(ChatHubGroups.User)).SendAsync(eventName, payload, cancellationToken);
    public async Task MessageCreatedAsync(
        MessageDto message,
        IReadOnlyCollection<int> recipientUserIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.Groups(recipientUserIds.Select(ChatHubGroups.User))
                .SendAsync("MessageReceived", message, cancellationToken);
        }
        catch (Exception exception)
        {
            // The message has already been persisted. Clients will receive it on their next history refresh.
            logger.LogWarning(exception, "Could not publish real-time event for message {MessageId}", message.Id);
        }
    }

    public async Task MessagesReadAsync(
        int chatId,
        int readerUserId,
        IReadOnlyCollection<int> messageIds,
        IReadOnlyCollection<int> recipientUserIds,
        CancellationToken cancellationToken = default)
    {
        if (messageIds.Count == 0) return;

        try
        {
            await hubContext.Clients.Groups(recipientUserIds.Select(ChatHubGroups.User))
                .SendAsync("MessagesRead", new { chatId, readerUserId, messageIds }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not publish read event for chat {ChatId}", chatId);
        }
    }
    public Task MessageUpdatedAsync(MessageDto message, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => PublishAsync("MessageUpdated", message, ids, cancellationToken);
    public Task MessageDeletedAsync(int chatId, int messageId, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => PublishAsync("MessageDeleted", new { chatId, messageId }, ids, cancellationToken);
    public Task ChatClearedAsync(int chatId, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => PublishAsync("ChatCleared", new { chatId }, ids, cancellationToken);
    public Task ChatDeletedAsync(int chatId, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => PublishAsync("ChatDeleted", new { chatId }, ids, cancellationToken);
    public Task ProfilePhotoUpdatedAsync(int userId, int? profilePhotoId, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => PublishAsync("ProfilePhotoUpdated", new { userId, profilePhotoId }, ids, cancellationToken);
    public Task UserPresenceChangedAsync(int userId, bool isOnline, DateTime? lastSeenAt, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => PublishAsync("UserPresenceChanged", new { userId, isOnline, lastSeenAt }, ids, cancellationToken);
}
