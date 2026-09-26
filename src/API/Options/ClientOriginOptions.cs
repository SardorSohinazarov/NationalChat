using System.Text.RegularExpressions;

namespace API.Options;

/// <summary>
/// Web client origins allowed to call the API with credentials (the refresh cookie). Configured as
/// <c>Cors:AllowedOrigins</c>, for example <c>Cors__AllowedOrigins__0=https://milliychat.uz</c>. A <c>*</c> stands
/// for one DNS label part, for Vercel preview URLs:
/// <c>https://national-chat-client-*-sardors-projects-56e94522.vercel.app</c>.
/// </summary>
public sealed class ClientOriginOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Only when the origins are known may the refresh cookie be sent cross-site: with any origin allowed and a
    /// SameSite=None cookie, any website could call /api/auth/refresh and read the new access token.
    /// </summary>
    public bool IsRestricted => AllowedOrigins.Any(origin => !string.IsNullOrWhiteSpace(origin));

    public bool IsAllowed(string origin) => AllowedOrigins.Any(pattern => Matches(pattern.Trim().TrimEnd('/'), origin));

    private static bool Matches(string pattern, string origin)
    {
        if (pattern.Length == 0) return false;
        if (!pattern.Contains('*')) return string.Equals(pattern, origin, StringComparison.OrdinalIgnoreCase);

        // "*" matches letters, digits and hyphens only, never a dot: it cannot stretch into another domain.
        var regex = "^" + Regex.Escape(pattern).Replace("\\*", "[a-z0-9-]*") + "$";
        return Regex.IsMatch(origin, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(50));
    }
}
