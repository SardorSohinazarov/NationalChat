using Application.Features.Stories.DataTransferObjects.Requests;
using Application.Features.Stories.DataTransferObjects.Responses;

namespace Application.Features.Stories;

public interface IStoryService
{
    Task<StoryFeedDto> GetFeedAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<StoryCreateResult> CreateAsync(int currentUserId, CreateStoryRequest request, CancellationToken cancellationToken = default);
    Task<ProtectedStoryMedia?> GetMediaAsync(int storyId, CancellationToken cancellationToken = default);
    Task<StoryDto?> ViewAsync(int currentUserId, int storyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoryViewerDto>?> GetViewersAsync(int currentUserId, int storyId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int currentUserId, int storyId, CancellationToken cancellationToken = default);
}

public sealed record StoryCreateResult(StoryDto? Story, string? Error);
public sealed record ProtectedStoryMedia(Stream Content, string MimeType, string FileName);
