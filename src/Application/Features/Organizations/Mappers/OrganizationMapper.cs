using System.Linq.Expressions;
using Application.Features.Organizations.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Organizations.Mappers;

public static class OrganizationMapper
{
    /// <summary>Badge of a loaded user; null when the membership (or its organization) is not loaded or absent.</summary>
    public static OrganizationBadgeDto? ToBadge(User user) =>
        user.OrganizationMembership?.Organization is { } organization
            ? new OrganizationBadgeDto(organization.Id, organization.ShortName)
            : null;

    public static OrganizationBadgeDto? ToBadge(Organization? organization) =>
        organization is null ? null : new OrganizationBadgeDto(organization.Id, organization.ShortName);

    public static Expression<Func<OrganizationMember, MyOrganizationDto>> MyOrganizationProjection => member =>
        new(member.Organization.Id, member.Organization.Name, member.Organization.ShortName, member.Role);
}
