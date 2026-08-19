namespace Application.Features.Stories.DataTransferObjects.Responses;

public sealed record StoryFeedUserDto(
    int UserId,
    string Username,
    string FirstName,
    string? LastName,
    int? ProfilePhotoId,
    bool HasUnseenStory,
    IReadOnlyList<StoryDto> Stories);
