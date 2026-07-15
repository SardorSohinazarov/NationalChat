namespace Domain.Entities;

public class Folder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public User User { get; set; }
    public ICollection<FolderChat> FolderChats { get; set; } = new List<FolderChat>();
}

public class FolderChat
{
    public int Id { get; set; }
    public int FolderId { get; set; }
    public int ChatId { get; set; }

    public Folder Folder { get; set; }
    public Chat Chat { get; set; }
}

public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal PricePaid { get; set; }

    public User User { get; set; }
}
