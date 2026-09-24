using Domain.Entities;

namespace Application.Features.Organizations.Factories;

public static class OrganizationFactory
{
    public static Organization Create(string name, string shortName, DateTime createdAt) =>
        new() { Name = name, ShortName = shortName, IsActive = true, CreatedAt = createdAt };

    public static OrganizationDomain CreateDomain(string domain) => new() { Domain = domain };

    public static OrganizationMember CreateMember(int organizationId, int userId, OrganizationRole role, DateTime verifiedAt) =>
        new() { OrganizationId = organizationId, UserId = userId, Role = role, VerifiedAt = verifiedAt };
}
