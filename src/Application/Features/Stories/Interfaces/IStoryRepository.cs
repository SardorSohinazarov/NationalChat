using Application.Abstractions.Persistence;
using Application.Features.Stories.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Stories;

public interface IStoryRepository : IBaseRepository<Story>
{
    Task<IReadOnlyList<Story>> GetActiveStoriesAsync(DateTime now, CancellationToken cancellationToken);
    Task<IReadOnlyList<Story>> GetAllByUserAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlySet<int>> GetViewedStoryIdsAsync(int viewerUserId, IReadOnlyCollection<int> storyIds, CancellationToken cancellationToken);
    Task<Story?> GetActiveStoryAsync(int storyId, DateTime now, CancellationToken cancellationToken);
    Task<Story?> GetOwnedStoryAsync(int storyId, int userId, CancellationToken cancellationToken);
    Task<Story?> GetStoryWithFileAsync(int storyId, CancellationToken cancellationToken);
    Task<bool> HasViewedAsync(int storyId, int viewerUserId, CancellationToken cancellationToken);
    Task AddViewAsync(StoryView view, CancellationToken cancellationToken);
    Task<IReadOnlyList<StoryViewerDto>> GetViewersAsync(int storyId, CancellationToken cancellationToken);
}
