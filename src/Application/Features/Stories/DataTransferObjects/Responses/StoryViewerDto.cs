namespace Application.Features.Stories.DataTransferObjects.Responses;

public sealed record StoryViewerDto(int UserId, string Username, string FirstName, string? LastName, int? ProfilePhotoId, DateTime ViewedAt);
