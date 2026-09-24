namespace Application.Features.Users.DataTransferObjects.Requests;

/// <param name="OrganizationId">When set, only verified members of this organization are returned (organization groups).</param>
public sealed record UserSearchRequest(string Query, int Limit = 20, int? OrganizationId = null);
