using Application.Features.Organizations.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Organizations;

public interface IOrganizationMembershipService
{
    /// <summary>
    /// Called after the user's e-mail has been verified. For an organization address (not a public mail service)
    /// it creates the organization and its common group if this is the first person from that domain — who
    /// becomes the admin — or otherwise joins the user to the organization and its auto-join groups.
    /// </summary>
    Task EnsureMembershipAsync(User user, CancellationToken cancellationToken = default);
}

public interface IOrganizationService
{
    Task<MyOrganizationDto?> GetMineAsync(int userId, CancellationToken cancellationToken = default);
}
