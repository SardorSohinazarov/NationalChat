namespace Application.Features.SecretChats.DataTransferObjects.Requests;

/// <param name="PublicKey">Base64 X25519 public key generated on the initiator's device.</param>
public sealed record CreateSecretChatRequest(int ParticipantId, string PublicKey);

/// <param name="PublicKey">Base64 X25519 public key generated on the accepting device.</param>
public sealed record AcceptSecretChatRequest(string PublicKey);

/// <param name="Seq">Sender's message counter; must grow with every message (it is also part of the AEAD associated data).</param>
/// <param name="Ciphertext">Base64 of the encrypted payload. The server never decrypts it.</param>
public sealed record SendSecretMessageRequest(long Seq, string Ciphertext);

/// <summary>Confirms that every message up to and including <paramref name="UpToId"/> reached this device.</summary>
public sealed record AckSecretMessagesRequest(long UpToId);

/// <summary>Delivery queue of this device, oldest first (it is a queue, not history).</summary>
public sealed record SecretMessagesQuery
{
    public long? AfterId { get; init; }

    public int Limit { get; init; } = 50;
}
