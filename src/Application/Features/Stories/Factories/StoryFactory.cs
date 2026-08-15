using Domain.Entities;

namespace Application.Features.Stories.Factories;

public static class StoryFactory
{
    public static Story Create(int userId, AttachmentType type, string? caption, DateTime now) =>
        new()
        {
            UserId = userId,
            Type = type,
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
            CreatedAt = now,
            ExpiresAt = now.AddHours(24),
        };
}
