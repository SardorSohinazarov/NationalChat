namespace Application.Features.Organizations.Options;

/// <summary>One entry of the "Organizations" configuration section (there is no admin panel).</summary>
public sealed class OrganizationOptions
{
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public List<string> Domains { get; init; } = [];
    public List<string> AdminEmails { get; init; } = [];
}

/// <summary>All configured organizations.</summary>
public sealed class OrganizationCatalog(IReadOnlyList<OrganizationOptions> organizations)
{
    public IReadOnlyList<OrganizationOptions> Organizations { get; } = organizations;

    public bool IsAdmin(string shortName, string email) =>
        Organizations.Any(organization =>
            string.Equals(organization.ShortName.Trim(), shortName, StringComparison.OrdinalIgnoreCase) &&
            organization.AdminEmails.Any(admin => string.Equals(admin.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase)));
}
