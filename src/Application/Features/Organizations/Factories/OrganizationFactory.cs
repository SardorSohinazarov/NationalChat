using Domain.Entities;

namespace Application.Features.Organizations.Factories;

public static class OrganizationFactory
{
    public static Organization Create(string domain, DateTime createdAt) =>
        new() { Domain = domain, ShortName = OrganizationEmailMatcher.ShortNameFor(domain), CreatedAt = createdAt };

    public static OrganizationMember CreateMember(int organizationId, int userId, OrganizationRole role, DateTime verifiedAt) =>
        new() { OrganizationId = organizationId, UserId = userId, Role = role, VerifiedAt = verifiedAt };
}
