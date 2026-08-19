namespace Application.Features.Stories.DataTransferObjects.Responses;

public sealed record StoryFeedDto(IReadOnlyList<StoryDto> MyStories, IReadOnlyList<StoryFeedUserDto> Others);
