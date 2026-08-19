using System.Linq.Expressions;
using Application.Features.Stories.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Stories.Mappers;

public static class StoryMapper
{
    public static StoryDto ToDto(Story story, bool isViewedByMe) =>
        new(story.Id, story.UserId, story.User.Username, story.User.FirstName, story.User.LastName, story.User.ProfilePhotoId,
            story.Caption, (int)story.Type, MediaUrl(story), story.CreatedAt, story.ExpiresAt, story.ViewCount, isViewedByMe);

    public static Expression<Func<StoryView, StoryViewerDto>> ViewerProjection => view =>
        new(view.UserId, view.User.Username, view.User.FirstName, view.User.LastName, view.User.ProfilePhotoId, view.ViewedAt);

    private static string MediaUrl(Story story) => $"/api/stories/{story.Id}/media";
}
