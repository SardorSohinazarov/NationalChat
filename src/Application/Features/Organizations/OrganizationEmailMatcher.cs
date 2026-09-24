namespace Application.Features.Organizations;

/// <summary>
/// Exact e-mail domain matching: the part after "@" must equal a registered domain.
/// "evil-tuit.uz" and "tuit.uz.evil.com" do not match "tuit.uz"; subdomains match only when listed themselves.
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

    public static bool Matches(string? email, string domain)
    {
        var emailDomain = ExtractDomain(email);
        return emailDomain is not null && emailDomain == NormalizeDomain(domain);
    }

    public static bool IsPublicEmailDomain(string? domain) =>
        NormalizeDomain(domain) is { } normalized && PublicEmailDomains.All.Contains(normalized);
}
