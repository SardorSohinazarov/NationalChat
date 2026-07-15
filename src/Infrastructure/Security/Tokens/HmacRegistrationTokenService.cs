using System.Security.Cryptography;
using System.Text;
using Application.Features.Authentication;
using Infrastructure.Security.Options;

namespace Infrastructure.Security.Tokens;

public sealed class HmacRegistrationTokenService : IRegistrationTokenService
{
    private readonly byte[] _key;

    public HmacRegistrationTokenService(AuthSecurityOptions options)
    {
        _key = HmacRefreshTokenHasher.ReadKey(options.RegistrationTokenSigningKey, nameof(options.RegistrationTokenSigningKey));
    }

    public string Create(string email, DateTime expiresAt)
    {
        var payload = $"{expiresAt.Ticks}:{email}";
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var signature = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(encodedPayload));
        return $"{encodedPayload}.{Base64UrlEncode(signature)}";
    }

    public string? Validate(string token, DateTime now)
    {
        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return null;
        }

        try
        {
            var expectedSignature = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(parts[0]));
            var actualSignature = Base64UrlDecode(parts[1]);
            if (!CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature))
            {
                return null;
            }

            var payload = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
            var separator = payload.IndexOf(':');
            if (separator <= 0 || !long.TryParse(payload[..separator], out var ticks))
            {
                return null;
            }

            var expiresAt = new DateTime(ticks, DateTimeKind.Utc);
            var email = payload[(separator + 1)..];
            return expiresAt > now && !string.IsNullOrWhiteSpace(email) ? email : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Convert.FromBase64String(padded);
    }
}
