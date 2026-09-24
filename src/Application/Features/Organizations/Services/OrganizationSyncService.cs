using Application.Features.Organizations.Factories;
using Application.Features.Organizations.Options;

namespace Application.Features.Organizations;

/// <summary>Makes the database match the "Organizations" configuration section.</summary>
public sealed class OrganizationSyncService(
    IOrganizationRepository repository,
    OrganizationCatalog catalog,
    TimeProvider timeProvider) : IOrganizationSyncService
{
    private const int NameMaxLength = 255;
    private const int ShortNameMaxLength = 32;

    public async Task<IReadOnlyList<string>> SyncAsync(CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();
        var desired = BuildDesiredState(warnings);
        var existing = await repository.GetAllWithDomainsAsync(cancellationToken);

        // Pass 1: remove domains that are no longer listed (or moved to another organization) and deactivate
        // organizations that left the configuration, so pass 2 can reuse their domains without unique conflicts.
        foreach (var organization in existing)
        {
            var wanted = desired.GetValueOrDefault(organization.ShortName);
            foreach (var domain in organization.Domains.ToArray())
            {
                if (wanted is not null && wanted.Domains.Contains(domain.Domain)) continue;
                organization.Domains.Remove(domain);
                repository.RemoveDomain(domain);
            }

            if (wanted is null && organization.IsActive)
            {
                organization.IsActive = false;
                await repository.RemoveMembersAsync(organization.Id, cancellationToken);
            }
        }

        await repository.SaveChangesAsync(cancellationToken);

        // Pass 2: create or update the configured organizations and add their new domains.
        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var (shortName, wanted) in desired)
        {
            var organization = existing.FirstOrDefault(x => x.ShortName == shortName);
            if (organization is null)
            {
                organization = OrganizationFactory.Create(wanted.Name, shortName, now);
                await repository.AddOrganizationAsync(organization, cancellationToken);
            }

            organization.Name = wanted.Name;
            organization.IsActive = true;
            foreach (var domain in wanted.Domains.Where(domain => organization.Domains.All(x => x.Domain != domain)))
            {
                organization.Domains.Add(OrganizationFactory.CreateDomain(domain));
            }
        }

        await repository.SaveChangesAsync(cancellationToken);
        return warnings;
    }

    private Dictionary<string, DesiredOrganization> BuildDesiredState(List<string> warnings)
    {
        var desired = new Dictionary<string, DesiredOrganization>(StringComparer.Ordinal);
        var claimedDomains = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var options in catalog.Organizations)
        {
            var name = options.Name.Trim();
            var shortName = options.ShortName.Trim();
            if (name.Length is 0 or > NameMaxLength || shortName.Length is 0 or > ShortNameMaxLength)
            {
                warnings.Add($"Tashkilot o'tkazib yuborildi: Name (1-{NameMaxLength}) va ShortName (1-{ShortNameMaxLength}) to'ldirilishi kerak (\"{options.ShortName}\").");
                continue;
            }

            if (desired.ContainsKey(shortName))
            {
                warnings.Add($"\"{shortName}\" tashkiloti konfiguratsiyada ikki marta berilgan; birinchisi olinadi.");
                continue;
            }

            var domains = new HashSet<string>(StringComparer.Ordinal);
            foreach (var rawDomain in options.Domains)
            {
                var domain = OrganizationEmailMatcher.NormalizeDomain(rawDomain);
                if (domain is null)
                {
                    warnings.Add($"{shortName}: \"{rawDomain}\" domeni noto'g'ri va rad etildi.");
                }
                else if (OrganizationEmailMatcher.IsPublicEmailDomain(domain))
                {
                    warnings.Add($"{shortName}: \"{domain}\" umumiy pochta xizmati, tashkilot domeni bo'la olmaydi.");
                }
                else if (claimedDomains.TryGetValue(domain, out var owner) && owner != shortName)
                {
                    warnings.Add($"{shortName}: \"{domain}\" domeni allaqachon {owner} tashkilotiga berilgan.");
                }
                else
                {
                    claimedDomains[domain] = shortName;
                    domains.Add(domain);
                }
            }

            desired[shortName] = new DesiredOrganization(name, domains);
        }

        return desired;
    }

    private sealed record DesiredOrganization(string Name, HashSet<string> Domains);
}
