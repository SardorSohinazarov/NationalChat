using Application.Features.Organizations.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Organizations;

public interface IOrganizationRepository
{
    /// <summary>All organizations (active or not) with their domains, tracked.</summary>
    Task<IReadOnlyList<Organization>> GetAllWithDomainsAsync(CancellationToken cancellationToken = default);
    Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);
    void RemoveDomain(OrganizationDomain domain);
    Task RemoveMembersAsync(int organizationId, CancellationToken cancellationToken = default);

    Task<Organization?> FindActiveByDomainAsync(string domain, CancellationToken cancellationToken = default);
    /// <summary>Tracked membership of the user with its organization, if any.</summary>
    Task<OrganizationMember?> GetMembershipAsync(int userId, CancellationToken cancellationToken = default);
    /// <summary>Adds and saves a membership; false when a concurrent request already created one for this user.</summary>
    Task<bool> TryAddMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default);
    void RemoveMember(OrganizationMember member);
    Task<IReadOnlyList<int>> GetAutoJoinGroupChatIdsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<MyOrganizationDto?> GetMyOrganizationAsync(int userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
