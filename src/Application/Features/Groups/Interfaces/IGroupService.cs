using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Groups.DataTransferObjects.Requests;
using Application.Features.Groups.DataTransferObjects.Responses;

namespace Application.Features.Groups;

public interface IGroupService
{
    Task<GroupDto?> GetAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default);
    Task<GroupResult> CreateAsync(int currentUserId, CreateGroupRequest request, CancellationToken cancellationToken = default);
    Task<GroupResult> UpdateAsync(int currentUserId, int chatId, UpdateGroupRequest request, CancellationToken cancellationToken = default);
    Task<GroupResult> UpdatePhotoAsync(int currentUserId, int chatId, StoreImageRequest request, CancellationToken cancellationToken = default);
    Task<GroupResult> AddMembersAsync(int currentUserId, int chatId, AddGroupMembersRequest request, CancellationToken cancellationToken = default);
    Task<GroupResult> RemoveMemberAsync(int currentUserId, int chatId, int userId, CancellationToken cancellationToken = default);
    Task<GroupResult> UpdateMemberRoleAsync(int currentUserId, int chatId, int userId, UpdateGroupMemberRoleRequest request, CancellationToken cancellationToken = default);
    Task<GroupLeaveResult> LeaveAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default);

    /// <summary>Creates (or replaces, revoking the old one) the group's invite link. Admins and the owner only.</summary>
    Task<GroupResult> CreateInviteLinkAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default);
    Task<GroupResult> RevokeInviteLinkAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default);
    Task<GroupInvitePreviewDto?> GetInvitePreviewAsync(int currentUserId, string token, CancellationToken cancellationToken = default);
    /// <summary>Anyone holding a valid invite link may join; this is the only way in besides being added by an admin.</summary>
    Task<GroupResult> JoinByInviteAsync(int currentUserId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the organization's private group (titled with its domain, e.g. "tuit.uz") owned by <paramref name="userId"/>,
    /// together with every verified member already known; false when the user has no organization.
    /// </summary>
    Task<bool> CreateOrganizationGroupAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Adds a verified organization member to an organization group (auto-join); false when not applicable.</summary>
    Task<bool> JoinViaOrganizationAsync(int chatId, int userId, CancellationToken cancellationToken = default);
}

public sealed record GroupResult(GroupDto? Group, string? Error);

public sealed record GroupLeaveResult(bool Succeeded, string? Error);
