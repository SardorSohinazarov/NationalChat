using System.Security.Cryptography;
using System.Text;
using Application.Authentication;

namespace Infrastructure.Authentication;

public sealed class HmacRefreshTokenHasher : IRefreshTokenHasher
{
    private readonly byte[] _key;

    public HmacRefreshTokenHasher(AuthSecurityOptions options)
    {
        _key = ReadKey(options.RefreshTokenHashKey, nameof(options.RefreshTokenHashKey));
    }

    public string Hash(string token) =>
        Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(token)));

    internal static byte[] ReadKey(string encodedKey, string optionName)
    {
        try
        {
            var key = Convert.FromBase64String(encodedKey);
            if (key.Length < 32)
            {
                throw new InvalidOperationException($"{optionName} kamida 32 bayt bo'lishi kerak.");
            }

            return key;
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"{optionName} Base64 formatida bo'lishi kerak.", exception);
        }
    }
}
