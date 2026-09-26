using Application.Features.SecretChats.DataTransferObjects.Requests;
using Application.Features.SecretChats.DataTransferObjects.Responses;

namespace Application.Features.SecretChats;

/// <summary>
/// Handshake and store-and-forward delivery for end-to-end encrypted chats. Every call acts as one device:
/// the current user together with the session (device) their access token belongs to.
/// </summary>
public interface ISecretChatService
{
    Task<IReadOnlyList<SecretChatDto>> ListAsync(int userId, int sessionId, CancellationToken cancellationToken = default);
    Task<SecretChatDto?> GetAsync(int userId, int sessionId, int secretChatId, CancellationToken cancellationToken = default);
    Task<SecretChatResult> CreateAsync(int userId, int sessionId, CreateSecretChatRequest request, CancellationToken cancellationToken = default);
    Task<SecretChatResult> AcceptAsync(int userId, int sessionId, int secretChatId, AcceptSecretChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>Either device closes the chat; the participant may also decline a pending request from any device.</summary>
    Task<SecretChatResult> CloseAsync(int userId, int sessionId, int secretChatId, CancellationToken cancellationToken = default);

    Task<SecretMessageResult> SendAsync(int userId, int sessionId, int secretChatId, SendSecretMessageRequest request, CancellationToken cancellationToken = default);
    Task<SecretMessagesResult> GetMessagesAsync(int userId, int sessionId, int secretChatId, SecretMessagesQuery query, CancellationToken cancellationToken = default);
    Task<SecretChatActionResult> AckAsync(int userId, int sessionId, int secretChatId, AckSecretMessagesRequest request, CancellationToken cancellationToken = default);

    /// <summary>Closes every chat bound to these sessions; called when they are logged out or revoked.</summary>
    Task CloseForSessionsAsync(IReadOnlyCollection<int> sessionIds, CancellationToken cancellationToken = default);

    /// <summary>Closes chats of expired or revoked sessions and stale requests, and drops old undelivered ciphertext.</summary>
    Task CleanupAsync(CancellationToken cancellationToken = default);
}

public sealed record SecretChatResult(SecretChatDto? Chat, string? Error);

/// <param name="Duplicate">The seq was already accepted (for example, a retry after a lost response).</param>
public sealed record SecretMessageResult(SecretMessageDto? Message, string? Error, bool Duplicate = false);

public sealed record SecretMessagesResult(SecretMessagesPage? Page, string? Error);

public sealed record SecretChatActionResult(bool Succeeded, string? Error);
