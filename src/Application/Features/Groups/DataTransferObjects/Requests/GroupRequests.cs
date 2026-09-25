using Domain.Entities;

namespace Application.Features.Groups.DataTransferObjects.Requests;

public sealed record CreateGroupRequest(string Title, string? Description, IReadOnlyCollection<int> MemberIds);

public sealed record UpdateGroupRequest(string Title, string? Description);

public sealed record AddGroupMembersRequest(IReadOnlyCollection<int> UserIds);

public sealed record UpdateGroupMemberRoleRequest(ChatMemberRole Role);
