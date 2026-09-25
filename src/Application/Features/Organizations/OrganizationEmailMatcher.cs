namespace Application.Features.Organizations;

/// <summary>
/// Turns a verified e-mail address into its organization: the whole domain after the last "@".
/// "ali@tuit.uz" → "tuit.uz", "vali@student.tuit.uz" → "student.tuit.uz" (a different organization).
/// Public mail services are not organizations.
/// </summary>
public static class OrganizationEmailMatcher
{
    /// <summary>Lower-case domain of <paramref name="email"/> without a trailing dot, or null when there is none.</summary>
    public static string? ExtractDomain(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var at = email.LastIndexOf('@');
        return at < 0 ? null : NormalizeDomain(email[(at + 1)..]);
    }

    /// <summary>Lower-case, trimmed domain without a trailing dot, or null when it is not a plausible domain.</summary>
    public static string? NormalizeDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) return null;
        var normalized = domain.Trim().TrimEnd('.').ToLowerInvariant();
        if (normalized.Length is 0 or > 253 || !normalized.Contains('.')) return null;
        if (normalized.Split('.').Any(label => label.Length == 0 || label.StartsWith('-') || label.EndsWith('-'))) return null;
        return normalized.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '.' or '-') ? normalized : null;
    }

    /// <summary>The organization domain of an e-mail, or null for public mail services and invalid addresses.</summary>
    public static string? OrganizationDomainOf(string? email) =>
        ExtractDomain(email) is { } domain && !PublicEmailDomains.All.Contains(domain) ? domain : null;

    public static bool IsPublicEmailDomain(string? domain) =>
        NormalizeDomain(domain) is { } normalized && PublicEmailDomains.All.Contains(normalized);
}
