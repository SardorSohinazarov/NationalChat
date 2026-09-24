using Application.Features.Organizations.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Organizations;

public interface IOrganizationMembershipService
{
    /// <summary>
    /// Brings the user's membership in line with their (already verified) e-mail: joins the matching organization,
    /// fixes the role, or drops a membership whose domain is no longer registered. A new member is added to the
    /// organization's auto-join groups.
    /// </summary>
    Task EnsureMembershipAsync(User user, CancellationToken cancellationToken = default);
}

public interface IOrganizationService
{
    Task<MyOrganizationDto?> GetMineAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IOrganizationSyncService
{
    /// <summary>Upserts configured organizations and domains; returns human-readable warnings for rejected entries.</summary>
    Task<IReadOnlyList<string>> SyncAsync(CancellationToken cancellationToken = default);
}
