namespace Application.Features.SecretChats;

/// <summary>Strict standard (padded) Base64 decoding for keys and ciphertext coming from clients.</summary>
public static class SecretChatBase64
{
    public static byte[]? TryDecode(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length % 4 != 0) return null;
        var buffer = new byte[value.Length / 4 * 3];
        return Convert.TryFromBase64String(value, buffer, out var written) ? buffer[..written] : null;
    }

    public static bool IsPublicKey(string? value) => TryDecode(value)?.Length == SecretChatLimits.PublicKeyBytes;
}
