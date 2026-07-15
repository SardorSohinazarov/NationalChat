using System.Security.Cryptography;

namespace API.Authentication;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "NationalChat";
    public string Audience { get; init; } = "NationalChat.Client";
    public string SigningKey { get; init; } = string.Empty;

    public byte[] GetSigningKey()
    {
        try
        {
            var key = Convert.FromBase64String(SigningKey);
            if (key.Length < 32)
            {
                throw new InvalidOperationException("Jwt:SigningKey kamida 32 bayt bo'lishi kerak.");
            }

            return key;
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("Jwt:SigningKey Base64 formatida bo'lishi kerak.", exception);
        }
    }
}
