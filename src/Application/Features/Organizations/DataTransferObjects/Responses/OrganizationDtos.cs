using Domain.Entities;

namespace Application.Features.Organizations.DataTransferObjects.Responses;

/// <summary>The "tuit.uz ✓" badge shown next to a verified member's name.</summary>
public sealed record OrganizationBadgeDto(int Id, string Domain);

public sealed record MyOrganizationDto(int Id, string Domain, OrganizationRole Role);
