using Application.Features.SecretChats.DataTransferObjects.Responses;

namespace Application.Features.SecretChats;

/// <summary>
/// Real-time events for secret chats. Once a chat is bound, events go to device sessions, never to every
/// device of a user. Implementations must not throw: the change is already committed.
/// </summary>
public interface ISecretChatRealtimeNotifier
{
    /// <summary>To every device of the participant, since the request is not bound to one yet.</summary>
    Task RequestedAsync(SecretChatDto participantView, int participantUserId, CancellationToken cancellationToken = default);

    /// <summary>To the initiator's device, and to all of the participant's devices so the others can hide the request.</summary>
    Task AcceptedAsync(SecretChatDto initiatorView, int initiatorSessionId, SecretChatDto participantView, int participantUserId, CancellationToken cancellationToken = default);

    Task MessageReceivedAsync(SecretMessageDto message, int recipientSessionId, CancellationToken cancellationToken = default);

    Task ClosedAsync(int secretChatId, IReadOnlyCollection<int> sessionIds, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default);
}
