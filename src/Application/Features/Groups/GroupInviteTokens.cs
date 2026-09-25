using System.Security.Cryptography;

namespace Application.Features.Groups;

/// <summary>Invite link tokens: 128 random bits, base64url (22 characters), impossible to guess.</summary>
public static class GroupInviteTokens
{
    private const int TokenLength = 22;

    public static string Create() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static bool IsWellFormed(string? token) =>
        token is { Length: TokenLength } && token.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_');
}
