using Domain.Entities;

namespace Application.Features.Organizations.DataTransferObjects.Responses;

/// <summary>The "TUIT ✓" badge shown next to a verified member's name.</summary>
public sealed record OrganizationBadgeDto(int Id, string ShortName);

public sealed record MyOrganizationDto(int Id, string Domain, string ShortName, OrganizationRole Role);
