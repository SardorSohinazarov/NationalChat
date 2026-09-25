using Application.Features.Organizations.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Groups.DataTransferObjects.Responses;

public sealed record GroupMemberDto(
    int Id,
    string Username,
    string FirstName,
    string? LastName,
    int? ProfilePhotoId,
    ChatMemberRole Role,
    bool IsOnline,
    DateTime? LastSeenAt,
    DateTime JoinedAt,
    OrganizationBadgeDto? Organization);

public sealed record GroupDto(
    int ChatId,
    string Title,
    string? Description,
    int? PhotoId,
    int CreatorId,
    ChatMemberRole MyRole,
    DateTime CreatedAt,
    IReadOnlyList<GroupMemberDto> Members,
    OrganizationBadgeDto? Organization,
    string? InviteToken);

/// <summary>What someone holding an invite link sees before joining; no member list.</summary>
public sealed record GroupInvitePreviewDto(int ChatId, string Title, string? Description, int MemberCount, bool IsMember);
