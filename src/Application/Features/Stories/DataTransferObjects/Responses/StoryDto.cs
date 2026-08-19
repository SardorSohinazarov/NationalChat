namespace Application.Features.Stories.DataTransferObjects.Responses;

public sealed record StoryDto(
    int Id,
    int UserId,
    string Username,
    string FirstName,
    string? LastName,
    int? ProfilePhotoId,
    string? Caption,
    int Type,
    string MediaUrl,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    int ViewCount,
    bool IsViewedByMe);
