using Application.Features.Groups;
using Application.Features.Organizations.Factories;
using Domain.Entities;

namespace Application.Features.Organizations;

public sealed class OrganizationMembershipService(
    IOrganizationRepository repository,
    IGroupService groupService,
    TimeProvider timeProvider) : IOrganizationMembershipService
{
    public async Task EnsureMembershipAsync(User user, CancellationToken cancellationToken = default)
    {
        // An e-mail never changes, so an existing membership is final.
        if (await repository.GetMembershipAsync(user.Id, cancellationToken) is not null) return;

        var domain = OrganizationEmailMatcher.OrganizationDomainOf(user.Email);
        if (domain is null) return;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var organization = await repository.FindByDomainAsync(domain, cancellationToken);
        var isFounder = false;
        if (organization is null)
        {
            var created = OrganizationFactory.Create(domain, now);
            isFounder = await repository.TryAddOrganizationAsync(created, cancellationToken);
            organization = isFounder ? created : await repository.FindByDomainAsync(domain, cancellationToken);
            if (organization is null) return;
        }

        // The first person from a domain becomes its admin.
        var role = isFounder ? OrganizationRole.Admin : OrganizationRole.Member;
        var member = OrganizationFactory.CreateMember(organization.Id, user.Id, role, now);
        if (!await repository.TryAddMemberAsync(member, cancellationToken)) return;

        var autoJoinChatIds = await repository.GetAutoJoinGroupChatIdsAsync(organization.Id, cancellationToken);
        if (autoJoinChatIds.Count == 0)
        {
            // First member (or the common group was deleted): create it with this user as its owner.
            await groupService.CreateOrganizationGroupAsync(user.Id, cancellationToken);
            return;
        }

        foreach (var chatId in autoJoinChatIds)
        {
            await groupService.JoinViaOrganizationAsync(chatId, user.Id, cancellationToken);
        }
    }
}
