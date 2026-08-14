using Application.Features.Messages.DataTransferObjects.Responses;

namespace Application.Features.Messages;

/// <summary>
/// Publishes events after a chat change has been committed to persistent storage.
/// Transport implementations belong outside the Application layer.
/// </summary>
public interface IChatRealtimeNotifier
{
    Task MessageCreatedAsync(
        MessageDto message,
        IReadOnlyCollection<int> recipientUserIds,
        CancellationToken cancellationToken = default);

    Task MessagesReadAsync(
        int chatId,
        int readerUserId,
        IReadOnlyCollection<int> messageIds,
        IReadOnlyCollection<int> recipientUserIds,
        CancellationToken cancellationToken = default);
    Task MessageUpdatedAsync(MessageDto message, IReadOnlyCollection<int> recipientUserIds, CancellationToken cancellationToken = default);
    Task MessageDeletedAsync(int chatId, int messageId, IReadOnlyCollection<int> recipientUserIds, CancellationToken cancellationToken = default);
    Task ChatClearedAsync(int chatId, IReadOnlyCollection<int> recipientUserIds, CancellationToken cancellationToken = default);
    Task ChatDeletedAsync(int chatId, IReadOnlyCollection<int> recipientUserIds, CancellationToken cancellationToken = default);
    Task ProfilePhotoUpdatedAsync(int userId, int? profilePhotoId, IReadOnlyCollection<int> recipientUserIds, CancellationToken cancellationToken = default);
}
