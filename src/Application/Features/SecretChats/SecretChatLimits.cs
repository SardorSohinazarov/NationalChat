namespace Application.Features.SecretChats;

public static class SecretChatLimits
{
    /// <summary>Raw X25519 public key length.</summary>
    public const int PublicKeyBytes = 32;

    /// <summary>Text only (4000 characters, up to 16 KB in UTF-8) plus the envelope and AES-GCM overhead.</summary>
    public const int MaxCiphertextBytes = 32 * 1024;

    public const int MaxPageSize = 100;

    /// <summary>A request nobody accepted is closed after this long.</summary>
    public static readonly TimeSpan PendingLifetime = TimeSpan.FromDays(7);

    /// <summary>Ciphertext the recipient device never fetched is dropped after this long.</summary>
    public static readonly TimeSpan UndeliveredRetention = TimeSpan.FromDays(7);
}
