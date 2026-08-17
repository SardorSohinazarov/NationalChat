using Application.Features.Stories;
using Application.Features.Stories.DataTransferObjects.Responses;
using Application.Features.Stories.Mappers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class StoryRepository(ChatDb db) : BaseRepository<Story>(db), IStoryRepository
{
    public async Task<IReadOnlyList<Story>> GetActiveStoriesAsync(DateTime now, CancellationToken cancellationToken) =>
        await Db.Stories.AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.ExpiresAt > now)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Story>> GetAllByUserAsync(int userId, CancellationToken cancellationToken) =>
        await Db.Stories.AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlySet<int>> GetViewedStoryIdsAsync(int viewerUserId, IReadOnlyCollection<int> storyIds, CancellationToken cancellationToken)
    {
        var ids = await Db.StoryViews.AsNoTracking()
            .Where(x => x.UserId == viewerUserId && storyIds.Contains(x.StoryId))
            .Select(x => x.StoryId)
            .ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    public Task<Story?> GetActiveStoryAsync(int storyId, DateTime now, CancellationToken cancellationToken) =>
        Db.Stories.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == storyId && x.ExpiresAt > now, cancellationToken);

    public Task<Story?> GetOwnedStoryAsync(int storyId, int userId, CancellationToken cancellationToken) =>
        Db.Stories.Include(x => x.User).Include(x => x.File)
            .FirstOrDefaultAsync(x => x.Id == storyId && x.UserId == userId, cancellationToken);

    public Task<Story?> GetStoryWithFileAsync(int storyId, CancellationToken cancellationToken) =>
        Db.Stories.AsNoTracking().Include(x => x.File)
            .FirstOrDefaultAsync(x => x.Id == storyId, cancellationToken);

    public Task<bool> HasViewedAsync(int storyId, int viewerUserId, CancellationToken cancellationToken) =>
        Db.StoryViews.AnyAsync(x => x.StoryId == storyId && x.UserId == viewerUserId, cancellationToken);

    public Task AddViewAsync(StoryView view, CancellationToken cancellationToken) =>
        Db.StoryViews.AddAsync(view, cancellationToken).AsTask();

    public async Task<IReadOnlyList<StoryViewerDto>> GetViewersAsync(int storyId, CancellationToken cancellationToken) =>
        await Db.StoryViews.AsNoTracking()
            .Where(x => x.StoryId == storyId)
            .OrderByDescending(x => x.ViewedAt)
            .Select(StoryMapper.ViewerProjection)
            .ToListAsync(cancellationToken);
}
