using File = Domain.Entities.File;

namespace Domain.Entities;

public class Story
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FileId { get; set; }
    public string? Caption { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ViewCount { get; set; }

    public User User { get; set; }
    public File File { get; set; }
    public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
}

public class StoryView
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public int UserId { get; set; }
    public DateTime ViewedAt { get; set; }

    public Story Story { get; set; }
    public User User { get; set; }
}
