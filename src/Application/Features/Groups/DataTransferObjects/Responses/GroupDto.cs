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
    DateTime JoinedAt);

public sealed record GroupDto(
    int ChatId,
    string Title,
    string? Description,
    int? PhotoId,
    int CreatorId,
    ChatMemberRole MyRole,
    DateTime CreatedAt,
    IReadOnlyList<GroupMemberDto> Members);
