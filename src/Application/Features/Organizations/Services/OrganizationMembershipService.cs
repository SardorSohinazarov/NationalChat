using Application.Features.Groups;
using Application.Features.Organizations.Factories;
using Application.Features.Organizations.Options;
using Domain.Entities;

namespace Application.Features.Organizations;

public sealed class OrganizationMembershipService(
    IOrganizationRepository repository,
    IGroupService groupService,
    OrganizationCatalog catalog,
    TimeProvider timeProvider) : IOrganizationMembershipService
{
    public async Task EnsureMembershipAsync(User user, CancellationToken cancellationToken = default)
    {
        var domain = OrganizationEmailMatcher.ExtractDomain(user.Email);
        var organization = domain is null || OrganizationEmailMatcher.IsPublicEmailDomain(domain)
            ? null
            : await repository.FindActiveByDomainAsync(domain, cancellationToken);
        var existing = await repository.GetMembershipAsync(user.Id, cancellationToken);

        if (organization is null)
        {
            // The domain is no longer registered: the badge goes away, group memberships stay.
            if (existing is not null)
            {
                repository.RemoveMember(existing);
                await repository.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var role = catalog.IsAdmin(organization.ShortName, user.Email) ? OrganizationRole.Admin : OrganizationRole.Member;
        if (existing?.OrganizationId == organization.Id)
        {
            if (existing.Role != role)
            {
                existing.Role = role;
                await repository.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        if (existing is not null)
        {
            repository.RemoveMember(existing);
            await repository.SaveChangesAsync(cancellationToken);
        }

        var member = OrganizationFactory.CreateMember(organization.Id, user.Id, role, timeProvider.GetUtcNow().UtcDateTime);
        if (!await repository.TryAddMemberAsync(member, cancellationToken)) return;

        // Auto-join runs only when the membership is new, so someone who left such a group is not pulled back in.
        foreach (var chatId in await repository.GetAutoJoinGroupChatIdsAsync(organization.Id, cancellationToken))
        {
            await groupService.JoinViaOrganizationAsync(chatId, user.Id, cancellationToken);
        }
    }
}
