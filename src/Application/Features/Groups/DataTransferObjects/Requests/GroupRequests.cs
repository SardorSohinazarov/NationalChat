using Domain.Entities;

namespace Application.Features.Groups.DataTransferObjects.Requests;

/// <param name="OrganizationOnly">Only verified members of the creator's organization may join.</param>
/// <param name="AutoJoin">Organization admins only: all current and future verified members are added automatically.</param>
public sealed record CreateGroupRequest(
    string Title,
    string? Description,
    IReadOnlyCollection<int> MemberIds,
    bool OrganizationOnly = false,
    bool AutoJoin = false);

public sealed record UpdateGroupRequest(string Title, string? Description);

public sealed record AddGroupMembersRequest(IReadOnlyCollection<int> UserIds);

public sealed record UpdateGroupMemberRoleRequest(ChatMemberRole Role);
