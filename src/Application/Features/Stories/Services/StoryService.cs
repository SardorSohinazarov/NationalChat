using Application.Features.Files;
using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Stories.DataTransferObjects.Requests;
using Application.Features.Stories.DataTransferObjects.Responses;
using Application.Features.Stories.Factories;
using Application.Features.Stories.Mappers;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Stories;

public sealed class StoryService(
    IStoryRepository repository,
    IFileService fileService,
    IValidator<CreateStoryRequest> createStoryValidator,
    TimeProvider timeProvider) : IStoryService
{
    private const int MaxVideoSizeBytes = 50 * 1024 * 1024;
    private static readonly HashSet<string> AllowedVideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/webm", "video/quicktime"
    };

    public async Task<StoryFeedDto> GetFeedAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var stories = await repository.GetActiveStoriesAsync(now, cancellationToken);
        var otherStoryIds = stories.Where(x => x.UserId != currentUserId).Select(x => x.Id).ToArray();
        var viewedIds = otherStoryIds.Length == 0
            ? new HashSet<int>()
            : (HashSet<int>)await repository.GetViewedStoryIdsAsync(currentUserId, otherStoryIds, cancellationToken);

        var myStories = stories.Where(x => x.UserId == currentUserId)
            .Select(x => StoryMapper.ToDto(x, isViewedByMe: true))
            .ToList();

        var others = stories.Where(x => x.UserId != currentUserId)
            .GroupBy(x => x.UserId)
            .Select(group =>
            {
                var user = group.First().User;
                var storyDtos = group.Select(x => StoryMapper.ToDto(x, viewedIds.Contains(x.Id))).ToList();
                return new StoryFeedUserDto(
                    user.Id, user.Username, user.FirstName, user.LastName, user.ProfilePhotoId,
                    storyDtos.Any(x => !x.IsViewedByMe), storyDtos);
            })
            .OrderByDescending(x => x.HasUnseenStory)
            .ThenByDescending(x => x.Stories.Max(story => story.CreatedAt))
            .ToList();

        return new StoryFeedDto(myStories, others);
    }

    public async Task<StoryCreateResult> CreateAsync(int currentUserId, CreateStoryRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await createStoryValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return new(null, string.Join(" ", validation.Errors.Select(x => x.ErrorMessage)));

        var contentType = request.DeclaredContentType ?? string.Empty;
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            if (!AllowedVideoContentTypes.Contains(contentType)) return new(null, "Faqat MP4, WebM yoki MOV formatidagi video qabul qilinadi.");
            if (request.Content.Length > MaxVideoSizeBytes) return new(null, "Video hajmi 50 MB dan oshmasligi kerak.");
            return await StoreAsync(currentUserId, request, AttachmentType.Video, cancellationToken);
        }

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return await StoreAsync(currentUserId, request, AttachmentType.Photo, cancellationToken);
        }

        return new(null, "Faqat rasm yoki video yuklash mumkin.");
    }

    private async Task<StoryCreateResult> StoreAsync(int currentUserId, CreateStoryRequest request, AttachmentType type, CancellationToken cancellationToken)
    {
        var storedResult = type == AttachmentType.Photo
            ? await fileService.StoreImageAsync(new StoreImageRequest(request.FileName, request.DeclaredContentType, request.Content), cancellationToken)
            : await fileService.StoreFileAsync(new StoreFileRequest(request.FileName, request.DeclaredContentType, request.Content), cancellationToken);
        if (storedResult.File is null) return new(null, storedResult.Error);
        var stored = storedResult.File;

        try
        {
            var file = new Domain.Entities.File { Name = stored.FileName, MimeType = stored.MimeType, SizeBytes = stored.SizeBytes, StoragePath = stored.StoragePath };
            var story = StoryFactory.Create(currentUserId, type, request.Caption, timeProvider.GetUtcNow().UtcDateTime);
            story.File = file;
            await repository.AddAsync(story, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            var persisted = await repository.GetOwnedStoryAsync(story.Id, currentUserId, cancellationToken) ?? story;
            return new(StoryMapper.ToDto(persisted, isViewedByMe: true), null);
        }
        catch
        {
            await fileService.DeleteAsync(stored.StoragePath, cancellationToken);
            throw;
        }
    }

    public async Task<ProtectedStoryMedia?> GetMediaAsync(int storyId, CancellationToken cancellationToken = default)
    {
        var story = await repository.GetStoryWithFileAsync(storyId, cancellationToken);
        if (story?.File is null) return null;
        var stream = await fileService.OpenReadAsync(story.File.StoragePath, cancellationToken);
        return stream is null ? null : new(stream, story.File.MimeType, story.File.Name);
    }

    public async Task<StoryDto?> ViewAsync(int currentUserId, int storyId, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var story = await repository.GetActiveStoryAsync(storyId, now, cancellationToken);
        if (story is null) return null;

        if (story.UserId == currentUserId) return StoryMapper.ToDto(story, isViewedByMe: true);

        if (!await repository.HasViewedAsync(storyId, currentUserId, cancellationToken))
        {
            await repository.AddViewAsync(new StoryView { StoryId = storyId, UserId = currentUserId, ViewedAt = now }, cancellationToken);
            story.ViewCount++;
            await repository.SaveChangesAsync(cancellationToken);
        }

        return StoryMapper.ToDto(story, isViewedByMe: true);
    }

    public async Task<IReadOnlyList<StoryViewerDto>?> GetViewersAsync(int currentUserId, int storyId, CancellationToken cancellationToken = default)
    {
        var story = await repository.GetOwnedStoryAsync(storyId, currentUserId, cancellationToken);
        if (story is null) return null;
        return await repository.GetViewersAsync(storyId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int currentUserId, int storyId, CancellationToken cancellationToken = default)
    {
        var story = await repository.GetOwnedStoryAsync(storyId, currentUserId, cancellationToken);
        if (story is null) return false;
        var storagePath = story.File.StoragePath;
        repository.Remove(story);
        await repository.SaveChangesAsync(cancellationToken);
        await fileService.DeleteAsync(storagePath, cancellationToken);
        return true;
    }
}
