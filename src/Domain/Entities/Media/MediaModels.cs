namespace Domain.Entities;

public class Photo
{
    public int Id { get; set; }
    public int FileId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public File File { get; set; }
}

public class Sticker
{
    public int Id { get; set; }
    public int SetId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public int FileId { get; set; }

    public File File { get; set; }
    public StickerSet StickerSet { get; set; }
}

public class StickerSet
{
    public int Id { get; set; }
    public int CreatorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsAnimated { get; set; }

    public User Creator { get; set; }
    public ICollection<Sticker> Stickers { get; set; } = new List<Sticker>();
}

public class File
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
