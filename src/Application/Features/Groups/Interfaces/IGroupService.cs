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
}

public sealed record GroupResult(GroupDto? Group, string? Error);

public sealed record GroupLeaveResult(bool Succeeded, string? Error);
