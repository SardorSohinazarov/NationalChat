using Domain.Entities;

namespace Application.Features.SecretChats.DataTransferObjects.Responses;

public sealed record SecretChatPeerDto(int Id, string Username, string FirstName, string? LastName, int? ProfilePhotoId);

/// <param name="IsInitiator">Whether the viewer started the chat.</param>
/// <param name="MySessionId">
/// Viewer's device the chat is bound to; null while an incoming request is pending. A device whose own
/// session id differs must not show the chat (it was accepted on another device).
/// </param>
public sealed record SecretChatDto(
    int Id,
    SecretChatStatus Status,
    bool IsInitiator,
    SecretChatPeerDto Peer,
    int? MySessionId,
    string? MyPublicKey,
    string? PeerPublicKey,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime? ClosedAt);

public sealed record SecretMessageDto(long Id, int SecretChatId, long Seq, string Ciphertext, DateTime CreatedAt);

public sealed record SecretMessagesPage(IReadOnlyList<SecretMessageDto> Items, long? NextCursor, bool HasMore);
