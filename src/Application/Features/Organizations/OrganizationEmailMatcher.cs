namespace Application.Features.Organizations;

/// <summary>
/// Turns a verified e-mail address into its organization domain. Nobody registers organizations: every
/// non-public mail domain is one, so ali@tuit.uz and vali@student.tuit.uz both belong to "tuit.uz".
/// </summary>
public static class OrganizationEmailMatcher
{
    // Second-level labels under which a registrant gets the third level, e.g. urdu.edu.uz or company.co.uk.
    private static readonly HashSet<string> SecondLevelLabels = new(StringComparer.Ordinal)
    {
        "ac", "co", "com", "edu", "gov", "mil", "net", "org"
    };

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

    /// <summary>
    /// The organization an e-mail belongs to: its registrable domain ("student.tuit.uz" → "tuit.uz",
    /// "urdu.edu.uz" stays), or null for public mail services and invalid addresses.
    /// </summary>
    public static string? OrganizationDomainOf(string? email)
    {
        var domain = ExtractDomain(email);
        if (domain is null) return null;

        var labels = domain.Split('.');
        var take = labels.Length >= 3 && SecondLevelLabels.Contains(labels[^2]) ? 3 : 2;
        var organizationDomain = string.Join('.', labels[^Math.Min(take, labels.Length)..]);
        return IsPublicEmailDomain(domain) || IsPublicEmailDomain(organizationDomain) ? null : organizationDomain;
    }

    /// <summary>Badge text for an organization domain: "tuit.uz" → "TUIT", "urdu.edu.uz" → "URDU".</summary>
    public static string ShortNameFor(string organizationDomain)
    {
        var label = organizationDomain.Split('.')[0].ToUpperInvariant();
        return label.Length <= 32 ? label : label[..32];
    }

    public static bool IsPublicEmailDomain(string? domain) =>
        NormalizeDomain(domain) is { } normalized && PublicEmailDomains.All.Contains(normalized);
}
