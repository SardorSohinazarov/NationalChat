using Application.Features.Organizations.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Organizations;

public interface IOrganizationRepository
{
    Task<Organization?> FindByDomainAsync(string domain, CancellationToken cancellationToken = default);
    /// <summary>Adds and saves an organization; false when a concurrent sign-in already created one for this domain.</summary>
    Task<bool> TryAddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);
    Task<OrganizationMember?> GetMembershipAsync(int userId, CancellationToken cancellationToken = default);
    /// <summary>Adds and saves a membership; false when a concurrent sign-in already created one for this user.</summary>
    Task<bool> TryAddMemberAsync(OrganizationMember member, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetAutoJoinGroupChatIdsAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<MyOrganizationDto?> GetMyOrganizationAsync(int userId, CancellationToken cancellationToken = default);
}
